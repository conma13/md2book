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

        // Steps:
        //     LoadFilesStep
        //     SortFilesStep
        //     ExtractHeadingsStep
        //     BuildTocStep
        //     BuildTitlePage
        //     ConvertMarkdownStep
        //     BuildPdfStep
        public BuildPdfPipeline(LoadFilesStep load)
        {
            _steps = new IPipelineStep<BuildContext>[] { load };
        }

        public void Run(BuildContext ctx, ILogger logger)
        { 
            foreach (var step in _steps) 
                step.Execute(ctx, logger);
        }
    }
}
