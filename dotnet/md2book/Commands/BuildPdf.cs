using md2book.Models;
using md2book.Pipeline;
using System.CommandLine;

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
                var inputFolder = parseResult.GetValue(GlobalOptions.Input)!;
                var titleFile = parseResult.GetValue(GlobalOptions.TitleFile);
                var title = parseResult.GetValue(GlobalOptions.Title);

                if (string.IsNullOrWhiteSpace(titleFile))
                {
                    titleFile = null;
                    title ??= Path.GetFileName(Path.GetFullPath(inputFolder));
                }

                var ctx = new BuildContext
                {
                    
                    InputFolder = inputFolder,
                    OutputFile = parseResult.GetValue(GlobalOptions.Output)!,
                    Title = title,
                    TitleFile = titleFile,
                    TocLevel = parseResult.GetValue(GlobalOptions.TOCLevel),
                };

                pipeline.Run(ctx);
            });

        }
    }
}
