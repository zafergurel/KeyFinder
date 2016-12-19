using System;

namespace KeyFinder
{
    class KeyMatch
    {
        public string Line { get; set; }
        public string File { get; set; }
        public int LineNumber { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public bool IsVariable { get; set; }
    }
}
