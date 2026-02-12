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
            : base("pdf", "Create pdf e-book")
        {
            Options.Add(GlobalOptions.Input);
            Options.Add(GlobalOptions.Output);
            Options.Add(GlobalOptions.Title);
            Options.Add(GlobalOptions.TitleFile);
            Options.Add(GlobalOptions.TOCLevel);

            SetAction((ParseResult parseResult) =>
            {
                var ctx = new BuildContext
                {
                    InputFolder = parseResult.GetValue<string>(GlobalOptions.Input),
                    OutputFile = parseResult.GetValue<string>(GlobalOptions.Output),
                    //TODO Add Title calculation if titleOpt and titlefileOpt are both empty
                    Title = parseResult.GetValue<string>(GlobalOptions.Title),
                    TitleFile = parseResult.GetValue<string>(GlobalOptions.TitleFile),
                    TocLevel = parseResult.GetValue<uint>(GlobalOptions.TOCLevel),
                };

                pipeline.Run(ctx);
            });

        }
    }
}
