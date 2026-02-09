using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using md2book.Commands;
//using md2book.Pipeline;
//using md2book.Pipeline.Steps;
//using md2book.Services;

var services = new ServiceCollection();

// Services
//services.AddSingleton<IMarkdownParser, MarkdownParser>();

// Pipeline steps

// Pipeline

// Commands
services.AddSingleton<BuildCommand>();

var provider = services.BuildServiceProvider();

// CLI root
var root = new RootCommand("Markdown -> PDF builder");
root.Subcommands.Add(provider.GetRequiredService<BuildCommand>());

return root.Parse(args).Invoke();