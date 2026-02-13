using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace md2book.Models
{
    public static class GlobalOptions
    {
        public static readonly Option<string> Input = new("--input", "-i")
        {
            Description = """
                Path to the directory containing Markdown files,
                with or without a trailing slash. Files are processed
                in alphabetical order, including subdirectories.

                """,
            DefaultValueFactory = parseResult => ".",
            HelpName = "INPUT DIRECTORY",
        };
        public static readonly Option<string> Output = new("--output", "-o")
        {
            Description = """
            Output file name without extension. The format 
            extension will be added automatically.

            """,
            DefaultValueFactory = parseResult => "book",
            HelpName = "OUTPUT FILE",
        };
        public static readonly Option<string?> Title = new("--title")
        {
            Description = """
            Text for the title page; ignored if --titlefile 
            is provided. Use "" to disable page generation
                [default: basename of the input directory]
            """,
            HelpName = "string",
        };
        public static readonly Option<string?> TitleFile = new("--titlefile")
        {
            Description = """
                Path to a Markdown file containing the book's 
                title page.
                """,
            HelpName = "TITLE FILE",
        };
        public static readonly Option<uint> TOCLevel = new("--toclevel")
        {
            Description = """
            Markdown header level used to generate the TOC.
            Use 0 to disable.

            """,
            DefaultValueFactory = parseResult => 1,
            HelpName = "uint",
        };

        static GlobalOptions()
        {
            Input.AcceptLegalFilePathsOnly();
            Output.AcceptLegalFileNamesOnly();
            TitleFile.AcceptLegalFileNamesOnly();
        }
    }
}
