using md2book.Commands;
using md2book.Models;
using md2book.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Help;
using Microsoft.Extensions.Logging;
// using md2book.Pipeline.Steps;
// using md2book.Services;

namespace md2book
{
    public static class Program
    {
        static int Main(string[] args)
        {
            // Create service collection
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
            });

            // Add global options to service collections
            services.AddSingleton<GlobalOptions>();

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

            // CLI RootCommand
            var root = new RootCommand("Creates an e-book from multiple markdown files");

            // Add options to RootCommand
            var globals = provider.GetRequiredService<GlobalOptions>();
            foreach (var opt in globals.All)
                root.Options.Add(opt);

            // Add subcommands to RootCommand
            root.Subcommands.Add(provider.GetRequiredService<BuildPdf>());

            // Supress error message when run without command
            root.SetAction((ParseResult parseResult) => new HelpAction().Invoke(parseResult));

            return root.Parse(args).Invoke();
        }
    }
}