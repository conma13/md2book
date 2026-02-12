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
        private readonly Option<string> _inputOpt;
        private readonly Option<string> _outputOpt;
        private readonly Option<string> _titleOpt;
        private readonly Option<string> _titlefileOpt;
        private readonly Option<uint> _levelOpt;

        public BuildPdf(BuildPipeline pipeline,
                        Option<string> inputOpt,
                        Option<string> outputOpt,
                        Option<string> titleOpt,
                        Option<string> titlefileOpt,
                        Option<uint> levelOpt
            ) 
            : base("pdf", "Create pdf e-book")
        {
            _inputOpt = inputOpt;
            _outputOpt = outputOpt;
            _titleOpt = titleOpt;
            _titlefileOpt = titlefileOpt;
            _levelOpt = levelOpt;

            SetAction((ParseResult parseResult) =>
            {
                var ctx = new BuildContext
                {
                    InputFolder = parseResult.GetValue<string>(_inputOpt),
                    OutputFile = parseResult.GetValue<string>(_outputOpt),
                    //TODO Add Title calculation if titleOpt and titlefileOpt are both empty
                    Title = parseResult.GetValue<string>(_titleOpt),
                    TitleFile = parseResult.GetValue<string>(_titlefileOpt),
                    TocLevel = parseResult.GetValue<uint>(_levelOpt),
                };

                pipeline.Run(ctx);
            });

        }
    }
}
