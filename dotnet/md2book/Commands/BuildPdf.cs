using md2book.Models;
using md2book.Pipeline;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Reflection.Emit;
using System.Text;

namespace md2book.Commands
{
    public class BuildPdf : Command
    {
        public BuildPdf(BuildPipeline pipeline) 
            : base("pdf", "Create pdf file")
        {
            this.SetAction((ParseResult parseResult) =>
            {
                var ctx = new BuildContext
                {
                    InputFolder = parseResult.GetValue<string>("--input"),
                    OutputFile = parseResult.GetValue<string>("--output"),
                    Title = parseResult.GetValue<string>("--title"),
                    TocLevel = parseResult.GetValue<int>("--toclevel")
                };

                pipeline.Run(ctx);
            });

        }
    }
}
