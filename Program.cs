using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            var finder = new Finder();
            if (finder.Initialize())
            {
                finder.Run();
            }
            Console.In.ReadLine();
            finder.Dispose();
        }
    }
}
