using System;
using System.Collections.Generic;
using System.Text;
using md2book.Models;
using Microsoft.Extensions.Logging;

namespace md2book.Pipeline.Steps
{
    public class LoadFilesStep : IPipelineStep<BuildContext>
    {
        private readonly ILogger<LoadFilesStep> _logger;

        public LoadFilesStep(ILogger<LoadFilesStep> logger)
        {
            _logger = logger;
        }

        public void Execute(BuildContext ctx)
        {
            var files = Directory.GetFiles(ctx.InputFolder, "*.md");

            foreach (var file in files)
            {
                var doc = new MarkdownDocument
                {
                    FilePath = file,
                    Content = File.ReadAllText(file)
                };

                // TODO Comparing filename to 'maybe' pathname. Need to convert both to full pathname
                if (!string.IsNullOrEmpty(ctx.TitleFile) &&
                    Path.GetFileName(file).Equals(ctx.TitleFile, StringComparison.OrdinalIgnoreCase))
                {
                    ctx.TitleDocument = doc;
                }
                else
                {
                    ctx.Documents.Add(doc);
                }
            }

            string logstr = $$"""
            Loaded {{ctx.Documents.Count}} files.
            {{(!(ctx.TitleDocument is null)
                ? "Found title page file " + ctx.TitleDocument.FilePath
                : "Title page file not found.")}}
            """;

            _logger.LogInformation(logstr);
        }
    }
}
