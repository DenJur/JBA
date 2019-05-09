using System.Collections.Generic;
using CommandLine;

namespace JBA
{
    public class Options
    {
        [Option('r', "read", Required = true, HelpText = "Input files to be processed.")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option("raw", Default = false, HelpText = "Use raw SQL query to insert data instead of EFCore.")]
        public bool Raw { get; set; }
    }
}