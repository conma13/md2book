using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using md2book.Commands;
using md2book.Pipeline;
// using md2book.Pipeline.Steps;
// using md2book.Services;

namespace md2book
{
    public static class Program
    {
        static int Main(string[] args)
        {
            var services = new ServiceCollection();

            // Services
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

            // Pipelines
            services.AddSingleton<BuildPipeline>();

            // Commands
            services.AddSingleton<BuildPdf>();

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // CLI root
            var root = new RootCommand("Creates an e-book from multiple markdown files");

            //// Supress error message when run without command
            //root.SetAction((ParseResult parseResult) =>
            //{
            //    new HelpAction().Invoke(parseResult);
            //    return 0;
            //});

            // Add shared arguments and options
            var inputOpt = new Argument<string>("--input")
            {
                Description = "Input folder",
                DefaultValueFactory = parseResult => "."
            };
            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Output file name",
                Required = true
            };
            var titleOpt = new Option<string>("--title")
            {
                DefaultValueFactory = parseResult => "Document",
                Description = "Title for the book"
            };
            var levelOpt = new Option<int>("--toclevel")
            {
                DefaultValueFactory = parseResult => 2,
                Description = "Heading level for TOC"
            };

            root.Arguments.Add(inputOpt);
            root.Options.Add(outputOpt);
            root.Options.Add(titleOpt);
            root.Options.Add(levelOpt);

            // Add subcommands
            root.Subcommands.Add(provider.GetRequiredService<BuildPdf>());

            return root.Parse(args).Invoke();
        }
    }
}