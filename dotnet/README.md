## dotnet

**Copilot drafts. Not tested or reviewed.**

**Pattern:** Command + Pipeline/Processor + Services

### Used packages
   |  |  |
   | --- | --- |
   | Dependency Injection | Microsoft.Extensions.DependencyInjection |
   | CLI | System.CommandLine |
   | Markdown to HTML | Markdig |
   | HTML to PDF | QuestPDF |
   |  |  |

### Project structure
   ```
   /md2book
      Program.cs
      Commands/
         BuildCommand.cs
      Pipeline/
         BuildPipeline.cs
         Steps/
            LoadFilesStep.cs
            SortFilesStep.cs
            ExtractHeadingsStep.cs
            BuildTocStep.cs
            ConvertMarkdownStep.cs
            BuildPdfStep.cs
   Services/
      IMarkdownParser.cs
      MarkdownParser.cs
      IHtmlRenderer.cs
      HtmlRenderer.cs
      IPdfBuilder.cs
      PdfBuilder.cs
   Models/
      BuildContext.cs
      MarkdownDocument.cs
      TocEntry.cs
   ```

### Main program - Program.cs
   ```c#
   using Microsoft.Extensions.DependencyInjection;
   using System.CommandLine;
   using MarkdownPdfTool.Commands;
   using MarkdownPdfTool.Pipeline;
   using MarkdownPdfTool.Pipeline.Steps;
   using MarkdownPdfTool.Services;

   var services = new ServiceCollection();

   // Services
   services.AddSingleton<IMarkdownParser, MarkdownParser>();
   services.AddSingleton<IHtmlRenderer, HtmlRenderer>();
   services.AddSingleton<IPdfBuilder, PdfBuilder>();

   // Pipeline steps
   services.AddSingleton<LoadFilesStep>();
   services.AddSingleton<SortFilesStep>();
   services.AddSingleton<ExtractHeadingsStep>();
   services.AddSingleton<BuildTocStep>();
   services.AddSingleton<ConvertMarkdownStep>();
   services.AddSingleton<BuildPdfStep>();

   // Pipeline
   services.AddSingleton<BuildPipeline>();

   // Commands
   services.AddSingleton<BuildCommand>();

   var provider = services.BuildServiceProvider();

   // CLI root
   var root = new RootCommand("Markdown -> PDF builder");
   root.AddCommand(provider.GetRequiredService<BuildCommand>());

   return root.Invoke(args);
   ```

### Command class - BuildCommand.cs
   ```c#
   using System.CommandLine;
   using MarkdownPdfTool.Models;
   using MarkdownPdfTool.Pipeline;

   namespace MarkdownPdfTool.Commands;

   public class BuildCommand : Command
   {
      public BuildCommand(BuildPipeline pipeline)
         : base("build", "Build PDF from markdown files")
      {
         var inputOpt = new Option<string>("--input", "Input folder") { IsRequired = true };
         var outputOpt = new Option<string>("--output", "Output PDF file") { IsRequired = true };
         var titleOpt = new Option<string>("--title", () => "Document", "Title for PDF");
         var levelOpt = new Option<int>("--level", () => 2, "Heading level for TOC");
         var titleFileOpt = new Option<string?>(
            "--title-file",
            "Markdown file to use as title page");

         AddOption(inputOpt);
         AddOption(outputOpt);
         AddOption(titleOpt);
         AddOption(levelOpt);
         AddOption(titleFileOpt);

         this.SetHandler((string input, string output, string title, int level, string& titleFile) =>
         {
               var ctx = new BuildContext
               {
                  InputFolder = input,
                  OutputPdf = output,
                  Title = title,
                  TocLevel = level,
                  TitleFile = titleFile
               };

               pipeline.Run(ctx);

         }, inputOpt, outputOpt, titleOpt, levelOpt, titleFileOpt);
      }
   }
   ```

### Pipeline - BuildPipeline.cs
   ```c#
   namespace MarkdownPdfTool.Pipeline;

   public interface IPipelineStep<TContext>
   {
      void Execute(TContext context);
   }

   public class BuildPipeline
   {
      private readonly IEnumerable<IPipelineStep<BuildContext>> _steps;

      public BuildPipeline(
         LoadFilesStep load,
         SortFilesStep sort,
         ExtractHeadingsStep extract,
         BuildTocStep toc,
         ConvertMarkdownStep convert,
         BuildPdfStep pdf)
      {
         _steps = new IPipelineStep<BuildContext>[] { load, sort, extract, toc, convert, pdf };
      }

      public void Run(BuildContext ctx)
      {
         foreach (var step in _steps)
               step.Execute(ctx);
      }
   }
   ```

### Pipelene steps
#### LoadFilesStep.cs
   ```c#
   using MarkdownPdfTool.Models;

   namespace MarkdownPdfTool.Pipeline.Steps;

   public class LoadFilesStep : IPipelineStep<BuildContext>
   {
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

            if (ctx.TitleFile != null &&
                  Path.GetFileName(file).Equals(ctx.TitleFile, StringComparison.OrdinalIgnoreCase))
            {
                  ctx.TitleDocument = doc;
            }
            else
            {
                  ctx.Documents.Add(doc);
            }
         }
      }
   }
   ```

#### SortFilesStep.cs
   ```c#
   namespace MarkdownPdfTool.Pipeline.Steps;

   public class SortFilesStep : IPipelineStep<BuildContext>
   {
      public void Execute(BuildContext ctx)
      {
         ctx.Documents = ctx.Documents
               .OrderBy(d => Path.GetFileName(d.FilePath))
               .ToList();
      }
   }
   ```

#### ExtractHeadingsStep.cs
   ```c#
   using MarkdownPdfTool.Services;

   namespace MarkdownPdfTool.Pipeline.Steps;

   public class ExtractHeadingsStep : IPipelineStep<BuildContext>
   {
      private readonly IMarkdownParser _parser;

      public ExtractHeadingsStep(IMarkdownParser parser)
      {
         _parser = parser;
      }

      public void Execute(BuildContext ctx)
      {
         foreach (var doc in ctx.Documents)
               doc.Headings = _parser.ExtractHeadings(doc.Content, ctx.TocLevel);
      }
   }
   ```

#### BuildTocStep.cs
   ```c#
   namespace MarkdownPdfTool.Pipeline.Steps;

   public class BuildTocStep : IPipelineStep<BuildContext>
   {
      public void Execute(BuildContext ctx)
      {
         int page = 0; // титульный лист — страница 0

         ctx.Toc = ctx.Documents
            .SelectMany(doc =>
            {
                  var entries = new List<TocEntry>();

                  foreach (var h in doc.Headings)
                  {
                     entries.Add(new TocEntry
                     {
                        Text = h.Text,
                        Level = h.Level,
                        Page = page
                     });
                  }

                  page++; // каждая глава — новая страница
                  return entries;
            })
            .ToList();
      }
   }
   ```

#### ConvertMarkdownStep.cs
   ```c#
   using MarkdownPdfTool.Services;

   namespace MarkdownPdfTool.Pipeline.Steps;

   public class ConvertMarkdownStep : IPipelineStep<BuildContext>
   {
      private readonly IHtmlRenderer _renderer;

      public ConvertMarkdownStep(IHtmlRenderer renderer)
      {
         _renderer = renderer;
      }

      public void Execute(BuildContext ctx)
      {
         foreach (var doc in ctx.Documents)
               doc.Html = _renderer.Render(doc.Content);
      }
   }
   ```

#### BuildPdfStep.cs
   ```c#
   using MarkdownPdfTool.Services;

   namespace MarkdownPdfTool.Pipeline.Steps;

   public class BuildPdfStep : IPipelineStep<BuildContext>
   {
      private readonly IPdfBuilder _pdf;

      public BuildPdfStep(IPdfBuilder pdf)
      {
         _pdf = pdf;
      }

      public void Execute(BuildContext ctx)
      {
         _pdf.Build(ctx);
      }
   }
   ```

### Services
#### MarkdownParser.cs (Markdig)
   ```c#
   using Markdig;
   using MarkdownPdfTool.Models;

   namespace MarkdownPdfTool.Services;

   public class MarkdownParser : IMarkdownParser
   {
      public List<TocEntry> ExtractHeadings(string md, int level)
      {
         var pipeline = new MarkdownPipelineBuilder().Build();
         var doc = Markdig.Markdown.Parse(md, pipeline);

         return doc
               .OfType<Markdig.Syntax.HeadingBlock>()
               .Where(h => h.Level == level)
               .Select(h => new TocEntry
               {
                  Text = h.Inline?.FirstChild?.ToString() ?? "",
                  Level = h.Level
               })
               .ToList();
      }
   }
   ```

#### HtmlRenderer.cs
   ```c#
   using Markdig;

   namespace MarkdownPdfTool.Services;

   public class HtmlRenderer : IHtmlRenderer
   {
      public string Render(string md)
      {
         var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
         return Markdig.Markdown.ToHtml(md, pipeline);
      }
   }
   ```

#### PdfBuilder.cs (минимальный QuestPDF)
   ```c#
   using QuestPDF.Fluent;
   using QuestPDF.Infrastructure;
   using MarkdownPdfTool.Models;

   namespace MarkdownPdfTool.Services;

   public class PdfBuilder : IPdfBuilder
   {
      public void Build(BuildContext ctx)
      {
         var titleHtml = ctx.TitleDocument != null
               ? ctx.TitleDocument.Html
               : $"<h1>{ctx.Title}</h1>";

         var tocEntries = ctx.Toc;

         Document.Create(container =>
         {
               container.Page(page =>
               {
                  page.Margin(40);

                  // --- TITILE PAGE ---
                  page.Content().Column(col =>
                  {
                     col.Item().PaddingBottom(200).Html(titleHtml);
                     col.Item().Text(ctx.Title).FontSize(32).Bold().AlignCenter();
                  });

                  page.Footer().AlignCenter().Text("1");
               });

               // --- TABLE OF CONTENTS ---
               container.Page(page =>
               {
                  page.Margin(40);

                  page.Header().Text("Оглавление").FontSize(24).Bold();

                  page.Content().Column(col =>
                  {
                     foreach (var entry in tocEntries)
                     {
                           col.Item().Row(row =>
                           {
                              row.RelativeItem(entry.Level * 10).Text(""); // отступ
                              row.RelativeItem(200).Text(entry.Text);
                              row.ConstantItem(40).AlignRight().Text(entry.Page.ToString());
                           });
                     }
                  });
               });

               // --- CONTENT PAGES ---
               foreach (var doc in ctx.Documents)
               {
                  container.Page(page =>
                  {
                     page.Margin(40);
                     page.Content().Html(doc.Html);
                  });
               }

         })
         .GeneratePdf(ctx.OutputPdf);
      }
   }
   ```

### Models
#### BuildContext.cs
   ```c#
   namespace MarkdownPdfTool.Models;

   public class BuildContext
   {
      public string InputFolder { get; set; }
      public string? TitleFile { get; set; }
      public MarkdownDocument? TitleDocument { get; set; }
      public string Title { get; set; }
      public int TocLevel { get; set; }
      public List<MarkdownDocument> Documents { get; set; } = new();
      public List<TocEntry> Toc { get; set; } = new();
      public string OutputPdf { get; set; }
   }
   ```

#### MarkdownDocument.cs
   ```c#
   namespace MarkdownPdfTool.Models;

   public class MarkdownDocument
   {
      public string FilePath { get; set; }
      public string Content { get; set; }
      public List<TocEntry> Headings { get; set; } = new();
      public string Html { get; set; }
   }
   ```

#### TocEntry.cs
   ```c#
   namespace MarkdownPdfTool.Models;

   public class TocEntry
   {
      public string Text { get; set; }
      public int Level { get; set; }
   }
   ```
