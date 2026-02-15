using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace md2book.Models
{
    public class GlobalOptions
    {
        public Option<string> Input { get; } = 
            new("--input", "-i")
            {
                Description = """
                    Path to the directory containing Markdown files,
                    with or without a trailing slash. Files are processed
                    in alphabetical order, including subdirectories.

                    """,
                DefaultValueFactory = parseResult => ".",
                HelpName = "INPUT DIRECTORY",
                Recursive = true,
            };
        
        public Option<string> Output { get; } = 
            new("--output", "-o")
            {
                Description = """
                Output file name without extension. The format 
                extension will be added automatically.

                """,
                DefaultValueFactory = parseResult => "book",
                HelpName = "OUTPUT FILE",
                Recursive = true,
            };
        
        public Option<string?> Title { get; } = 
            new("--title")
            {
                Description = """
                Text for the title page; ignored if --titlefile 
                is provided. Use "" to disable page generation
                    [default: basename of the input directory]
                """,
                HelpName = "string",
                Recursive = true,
            };
        
        public Option<string?> TitleFile { get; } = 
            new("--titlefile")
            {
                Description = """
                    Path to a Markdown file containing the book's 
                    title page.
                    """,
                HelpName = "TITLE FILE",
                Recursive = true,
            };
        
        public Option<uint> TOCLevel { get; } = 
            new("--toclevel")
            {
                Description = """
                Markdown header level used to generate the TOC.
                Use 0 to disable.

                """,
                DefaultValueFactory = parseResult => 1,
                HelpName = "uint",
                Recursive = true,
            };

        public IEnumerable<Option> All =>
            new Option[] { Input, Output, Title, TitleFile, TOCLevel };

        public GlobalOptions()
        {
            Input.AcceptLegalFilePathsOnly();
            Output.AcceptLegalFilePathsOnly();
            TitleFile.AcceptLegalFilePathsOnly();
        }
    }
}
