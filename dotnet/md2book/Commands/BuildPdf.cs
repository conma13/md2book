using md2book.Models;
using md2book.Pipeline;
using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace md2book.Commands
{
    public class BuildPdf : Command
    {
        private readonly GlobalOptions _globals;
        private readonly ILogger<BuildPdf> _logger;

        public BuildPdf(GlobalOptions globals, ILogger<BuildPdf> logger, BuildPdfPipeline pipeline) 
            : base("pdf", "Create pdf e-book")
        {
            _globals = globals;
            _logger = logger;

            SetAction((ParseResult parseResult) =>
            {
                var inputFolder = parseResult.GetValue(_globals.Input)!;
                var titleFile = parseResult.GetValue(_globals.TitleFile);
                var title = parseResult.GetValue(_globals.Title);

                if (string.IsNullOrWhiteSpace(titleFile))
                {
                    titleFile = null;
                    title ??= Path.GetFileName(Path.GetFullPath(inputFolder));
                }

                var ctx = new BuildContext
                {
                    
                    InputFolder = inputFolder,
                    OutputFile = parseResult.GetValue(_globals.Output)!,
                    Title = title,
                    TitleFile = titleFile,
                    TocLevel = parseResult.GetValue(_globals.TOCLevel),
                };

                string logstr = $$"""
                Creating "{{ctx.OutputFile}}.pdf" from files in "{{ctx.InputFolder}}"
                with title {{(string.IsNullOrWhiteSpace(ctx.TitleFile)
                                ? "\"" + ctx.Title
                                : "from \"" + ctx.TitleFile)}}"
                and TOC level {{ctx.TocLevel}}
                """;

                _logger.LogInformation(logstr);

                pipeline.Run(ctx, _logger);
            });

        }
    }
}
