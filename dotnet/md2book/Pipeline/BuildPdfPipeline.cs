using md2book.Commands;
using md2book.Models;
using md2book.Pipeline.Steps;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace md2book.Pipeline
{
    public class BuildPdfPipeline
    {
        private readonly IEnumerable<IPipelineStep<BuildContext>> _steps;
        private readonly ILogger _logger;

        // Steps:
        //     LoadFilesStep
        //     SortFilesStep
        //     ExtractHeadingsStep
        //     BuildTocStep
        //     BuildTitlePage
        //     ConvertMarkdownStep
        //     BuildPdfStep
        public BuildPdfPipeline(ILogger<BuildPdfPipeline> logger,
            LoadFilesStep load)
        {
            _steps = new IPipelineStep<BuildContext>[] { load };
            _logger = logger;
        }

        public void Run(BuildContext ctx)
        {
            _logger.LogInformation("Start pipeline {DateTime}", DateTime.Now.ToString());
            foreach (var step in _steps)
            {
                step.Execute(ctx);
            }
            _logger.LogInformation("Finish pipeline {DateTime}", DateTime.Now.ToString());
        }
    }
}
