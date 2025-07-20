using DotTimeWork.Commands;
using DotTimeWork.Infrastructure;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork
{
    internal class Program
    {
        private static readonly string[] HelpStrings = { "-?", "--help", "-h", "-H", "/h", "/?" };

        static async Task<int> Main(string[] args)
        {
            try
            {
                DisplayWelcomeMessage(args);
                InitializeApplication();
                
                var rootCommand = CreateRootCommand();
                return await rootCommand.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return 1;
            }
            finally
            {
                ServiceContainer.Dispose();
            }
        }

        private static void DisplayWelcomeMessage(string[] args)
        {
            if (args.Length == 0 || HelpStrings.Any(x => x.Equals(args[0], StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.Write(
                    new FigletText("DotTimeWork")
                        .LeftJustified()
                        .Color(Color.Red));
                AnsiConsole.MarkupLine($"[grey]Version {GlobalConstants.RELEASE_VERSION}[/]");
                AnsiConsole.WriteLine();
            }
        }

        private static void InitializeApplication()
        {
            PublicOptions.InitializeAliases();
            ServiceContainer.Initialize();
        }

        private static RootCommand CreateRootCommand()
        {
            var rootCommand = new RootCommand("Working Time Tracking Helper")
            {
                Description = "A tool to help you track your working time and manage your projects."
            };
            
            rootCommand.AddGlobalOption(PublicOptions.VerboseLogging);

            foreach (var command in ServiceContainer.GetAllCommands())
            {
                rootCommand.Add(command);
            }

            return rootCommand;
        }
    }
}
