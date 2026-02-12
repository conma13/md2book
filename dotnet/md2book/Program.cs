using md2book.Commands;
using md2book.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Help;
// using md2book.Pipeline.Steps;
// using md2book.Services;

namespace md2book
{
    public static class Program
    {
        static int Main(string[] args)
        {
            // Build shared options
            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = """
                Path to the directory containing Markdown files,
                with or without a trailing slash. Files are processed
                in alphabetical order, including subdirectories.

                """,
                DefaultValueFactory = parseResult => ".",
                HelpName = "INPUT DIRECTORY",
            };
            inputOpt.AcceptLegalFilePathsOnly();
            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = """
                Output file name without extension. The format 
                extension will be added automatically.

                """,
                DefaultValueFactory = parseResult => "book",
                HelpName = "OUTPUT FILE",
            };
            outputOpt.AcceptLegalFileNamesOnly();
            var titleOpt = new Option<string>("--title")
            {
                Description = """
                Text for the title page; ignored if --titlefile 
                is provided.
                 [default: basename of the input directory]
                """,
                HelpName = "string",
            };
            var titlefileOpt = new Option<string>("--titlefile")
            {
                Description = """
                Path to a Markdown file containing the book's 
                title page.
                """,
                HelpName = "TITLE FILE",
            };
            titlefileOpt.AcceptLegalFileNamesOnly();
            var levelOpt = new Option<uint>("--toclevel")
            {
                Description = """
                Markdown header level used to generate the TOC.
                Use 0 to disable.

                """,
                DefaultValueFactory = parseResult => 2,
                HelpName = "uint",
            };

            // Create service collection
            var services = new ServiceCollection();

            // Add options
            services.AddSingleton(inputOpt);
            services.AddSingleton(outputOpt);
            services.AddSingleton(titleOpt);
            services.AddSingleton(titlefileOpt);
            services.AddSingleton(levelOpt);

            // Add services
            // services.AddSingleton<IMarkdownParser, MarkdownParser>();
            // services.AddSingleton<IHtmlRenderer, HtmlRenderer>();
            // services.AddSingleton<IPdfBuilder, PdfBuilder>();

            // Pipeline steps
            // services.AddSingleton<LoadFilesStep>();
            // services.AddSingleton<SortFilesStep>();
            // services.AddSingleton<ExtractHeadingsStep>();
            // services.AddSingleton<BuildTocStep>();
            // services.AddSingleton<ConvertMarkdownStep>();
            // services.AddSingleton<BuildPdfStep>();

            // Add pipelines
            services.AddSingleton<BuildPipeline>();

            // Add commands
            services.AddSingleton<BuildPdf>();

            // Build service provider
            var provider = services.BuildServiceProvider();

            // CLI root
            var root = new RootCommand("Creates an e-book from multiple markdown files")
            {
                inputOpt,
                outputOpt,
                titleOpt,
                titlefileOpt,
                levelOpt,
                provider.GetRequiredService<BuildPdf>(),
            };

            // Supress error message when run without command
            root.SetAction((ParseResult parseResult) => new HelpAction().Invoke(parseResult));

            return root.Parse(args).Invoke();
        }
    }
}