using DotTimeWork.Commands;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork
{
    internal class Program
    {
        private static readonly string[] HelpStrings =
        {
            "-?",
            "--help",
            "-h"
        };
        static async Task<int> Main(string[] args)
        {
            if (args.Length == 0 || HelpStrings.Any(x=>x.Equals(args[0])))
            {
                AnsiConsole.Write(
                    new FigletText("DotTimeWork")
                        .LeftJustified()
                        .Color(Color.Red));
                AnsiConsole.MarkupLine($"[grey]Version {GlobalConstants.RELEASE_VERSION}[/]");
                AnsiConsole.WriteLine();
            }

            PublicOptions.InitOptions();
            DI.InitDependencyInjection();

            var rootCommand = new RootCommand("Working Time Tracking Helper")
            {
                Description = "A tool to help you track your working time and manage your projects."
            };
            rootCommand.AddGlobalOption(PublicOptions.VerboseLogging);

            foreach (var command in DI.GetAllCommands())
            {
                rootCommand.Add(command);
            }

            return await rootCommand.InvokeAsync(args);
        }
    }
}
