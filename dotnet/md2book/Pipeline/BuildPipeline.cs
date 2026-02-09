using md2book.Models;
using md2book.Pipeline.Steps;
using System;
using System.Collections.Generic;
using System.Text;

namespace md2book.Pipeline
{
    public class BuildPipeline
    {
        private readonly IEnumerable<IPipelineStep<BuildContext>> _steps;

        public BuildPipeline(IEnumerable<IPipelineStep<BuildContext>> steps)
        {
            _steps = steps;
        }

        public void Run(BuildContext ctx) 
        { 
            foreach (var step in _steps) 
                step.Execute(ctx);
        }
    }
}
