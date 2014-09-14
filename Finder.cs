using KeyFinder.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KeyFinder
{
    class Finder : IDisposable
    {
        public string BaseSearchFolder { get; set; }
        public List<string> SearchPaths { get; set; }
        public List<string> ExcludedPaths { get; set; }
        public string FileFilter { get; set; }
        public bool RecursiveSearchEnabled { get; set; }
        public string OutputFile { get; set; }
        public bool OpenOutputFileAfterProcess { get; set; }
        public string SearchPatternFile { get; set; }

        List<Regex> patterns;

        List<KeyMatch> resources;

        System.Collections.Concurrent.ConcurrentBag<KeyMatch> bag = new System.Collections.Concurrent.ConcurrentBag<KeyMatch>();

        public Finder()
        {

        }

        public bool Initialize()
        {
            resources = new List<KeyMatch>();
            BaseSearchFolder = ConfigurationManager.AppSettings["BaseSearchFolder"] ?? "";
            SearchPaths = (ConfigurationManager.AppSettings["SearchPaths"] ?? "").Split('|').ToList();
            FileFilter = ConfigurationManager.AppSettings["FileFilter"] ?? "*.*";
            ExcludedPaths = (ConfigurationManager.AppSettings["ExcludedPaths"] ?? "").Split('|').ToList();
            RecursiveSearchEnabled = (ConfigurationManager.AppSettings["RecursiveSearchEnabled"] ?? "true") == "true";
            OutputFile = ConfigurationManager.AppSettings["OutputFile"];
            OpenOutputFileAfterProcess = (ConfigurationManager.AppSettings["RecursiveSearchEnabled"] ?? "false") == "true";
            SearchPatternFile = ConfigurationManager.AppSettings["SearchPatternFile"];

            if (string.IsNullOrEmpty(FileFilter))
            {
                Console.WriteLine("Error: File filter is not defined.");
                return false;
            }

            if (SearchPaths.Count == 0 && string.IsNullOrEmpty(BaseSearchFolder))
            {
                Console.WriteLine("Error: Search paths and base search path could not be empty. At least define BaseSearchPath.");
                return false;
            }

            if (string.IsNullOrEmpty(SearchPatternFile))
            {
                Console.WriteLine("Error: Search pattern file is not defined");
                return false;
            }
            if (!File.Exists(SearchPatternFile))
            {
                Console.WriteLine("Error: Search pattern file could not be found at {0}.", SearchPatternFile);
                return false;
            }

            // get patterns
            patterns = new List<Regex>();
            string content = File.ReadAllText(SearchPatternFile);
            if (!string.IsNullOrEmpty(content))
                content.Split('\n').ToList().ForEach(t => patterns.Add(new Regex(t.TrimEnd('\r', '\n'))));


            return true;
        }

        public void Run()
        {
            var start = DateTime.Now;
            Console.Out.WriteLine("BaseSearchFolder: " + BaseSearchFolder);
            SearchPaths.ForEach(folder =>
            {
                if (!Path.IsPathRooted(folder) && !string.IsNullOrEmpty(BaseSearchFolder))
                {
                    folder = Path.Combine(BaseSearchFolder, folder);
                }
                int currentCount = bag.Count;
                Console.Out.WriteLine("Analyzing " + folder);
                if (Directory.Exists(folder))
                {
                    findResourcesInFolder(folder, folder);
                    Console.Out.WriteLine("   found " + (bag.Count - currentCount) + " matches...");
                }
                else
                {
                    Console.WriteLine("Error: The directory doesn't exist.");
                }
                Console.Out.WriteLine("");
            });
            Console.Out.WriteLine("Process was completed in " + (DateTime.Now.Subtract(start).TotalMilliseconds + " ms"));

            Task.Run(() => createOutputFile());
        }
        private void createOutputFile()
        {
            string ext = Path.GetExtension(OutputFile);
            if (ext == ".xlsx")
            {
                CreateExcelFile.CreateExcelDocument(bag.ToList(), OutputFile);
            }
            else
            {
                var xmlOutput = UtilityFunctions.ListToXml(bag.ToList(), "resources");
                File.WriteAllText(OutputFile, xmlOutput.InnerXml, UTF8Encoding.UTF8);
            }

            if (File.Exists(OutputFile) && OpenOutputFileAfterProcess)
            {
                Console.Out.WriteLine("Opening result Excel file (" + OutputFile + ")");
                Process.Start(OutputFile);
            }

        }

        private void findResourcesInFolder(string folder, string baseFolder)
        {
            List<string> files = new List<string>();

            FileFilter.Split(';').ToList().ForEach(filter => { files = files.Concat(Directory.GetFiles(folder, filter)).ToList(); });

            files.AsParallel().ForAll(f =>
            {
                using (StreamReader stream = new StreamReader(f))
                {
                    int lineCount = 1;
                    int resCount = 0;

                    string line = stream.ReadLine();

                    while (line != null)
                    {
                        if (line.Length < 250 && line.Length > 15)
                        {
                            patterns.ToList().ForEach(r =>
                            {
                                var matchCol = r.Matches(line);
                                for (int i = 0; i < matchCol.Count; i++)
                                {
                                    var m = matchCol[i];
                                    if (m.Groups["key"] != null)
                                    {
                                        addMatch(f, line, lineCount, m);
                                        ++resCount;
                                    }
                                }
                            });
                        }
                        ++lineCount;
                        line = stream.ReadLine();
                    }

                    Console.Out.WriteLine("*** " + f.Substring(baseFolder.Length) + " " + resCount);
                }

            });
            if (RecursiveSearchEnabled)
            {
                string[] directories = Directory.GetDirectories(folder);
                if (directories != null)
                {
                    directories
                        .Where(d => !ExcludedPaths.Any(p => p.Equals(Path.GetFileName(d), StringComparison.CurrentCultureIgnoreCase)) && !ExcludedPaths.Any(p => p.Equals(d, StringComparison.CurrentCultureIgnoreCase))).ToList()
                        .ForEach(d => findResourcesInFolder(d, baseFolder));
                }
            }
        }



        private void addMatch(string f, string line, int lineCount, Match m)
        {
            if (m.Groups.Count < 3) return;
            string val = m.Groups["key"].Value;
            bool isVariable = !val.StartsWith("\"") && !val.StartsWith("'");
            if (!isVariable) val = val.Substring(1, val.Length - 2);
            bag.Add(new KeyMatch
            {
                File = f,
                Line = line,
                LineNumber = lineCount,
                Key = val,
                Type = m.Groups["type"] != null ? m.Groups["type"].Value : "N/A",
                IsVariable = isVariable
            });
        }

        public void Dispose()
        {

        }
    }
}
