using md2book.Models;
using md2book.Pipeline;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Reflection.Emit;
using System.Text;

namespace md2book.Commands
{
    public class BuildCommand : Command
    {
        public BuildCommand(BuildPipeline pipeline) 
            : base("md2book", "Compile book from markdown files")
        {
            var inputOpt = new Argument<string>("--input")
            {
                Description = "Input folder",
                DefaultValueFactory = parseResult => @".\*.md"
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

            Arguments.Add(inputOpt);
            Options.Add(outputOpt);
            Options.Add(titleOpt);
            Options.Add(levelOpt);

            this.SetAction((ParseResult parseResult) =>
            {
                var ctx = new BuildContext
                {
                    InputFolder = parseResult.GetRequiredValue(inputOpt),
                    OutputFile = parseResult.GetRequiredValue(outputOpt),
                    Title = parseResult.GetValue(option: titleOpt),
                    TocLevel = parseResult.GetValue(option: levelOpt)
                };

                pipeline.Run(ctx);
            });

        }
    }
}
