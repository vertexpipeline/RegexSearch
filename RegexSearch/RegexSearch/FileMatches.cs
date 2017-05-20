using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RegexSearch
{
    class FileMatches
    {
        public List<Match> Matches;
        public string FileName { get; set; }

        public FileMatches(string name)
        {
            FileName = name;
        }
    }
}
