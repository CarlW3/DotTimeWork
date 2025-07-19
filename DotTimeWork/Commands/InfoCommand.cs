using DotTimeWork.Commands.Base;
using DotTimeWork.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class InfoCommand : BaseCommand
    {
        public InfoCommand() : base("Info", Properties.Resources.Info_Description)
        {
        }

        protected override void SetupCommand()
        {
            this.SetHandler(Execute, PublicOptions.VerboseLogging);
        }

        public void Execute(bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                DisplayHeader();
                DisplayDescription();
                DisplayFeatures();
                DisplayDevelopmentInfo();
            }, verboseLogging);
        }

        private static void DisplayHeader()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText($"DotTimeWork Version {GlobalConstants.RELEASE_VERSION}")
                    .Centered()
                    .Color(Color.Yellow));
            AnsiConsole.WriteLine();
        }

        private static void DisplayDescription()
        {
            AnsiConsole.MarkupLine(Properties.Resources.Info_FirstLine);
            AnsiConsole.MarkupLine(Properties.Resources.Info_SecondLine);
            AnsiConsole.MarkupLine(Properties.Resources.Info_Development);
            AnsiConsole.WriteLine();
        }

        private static void DisplayFeatures()
        {
            var features = GetFeaturesMarkup();
            var panel = new Panel(new Markup(features))
            {
                Border = BoxBorder.Heavy,
                Header = new PanelHeader("Included functions"),
                Expand = true,
                Padding = new Padding(2, 2, 2, 2),
                Width = 80,
            };
            
            AnsiConsole.Write(new Padder(panel).PadRight(5).PadLeft(5));
            AnsiConsole.WriteLine();
        }

        private static void DisplayDevelopmentInfo()
        {
            var rule = new Rule("[red]Development[/]");
            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine("[green]Developer: [/][bold]Carl-Philip Wenz[/]");
            
            if (ServiceContainer.IsInitialized)
            {
                var serviceCount = ServiceContainer.ServiceProvider.GetServices<object>().Count();
                AnsiConsole.MarkupLine($"[gray]Statistics (Active Services): [/]{serviceCount}");
            }
            
            AnsiConsole.MarkupLine("[gray]Development just started, so please be patient.[/]");
            AnsiConsole.WriteLine();
        }

        private static string GetFeaturesMarkup()
        {
            return """
                [green bold]Create Project[/]
                Creates Project configuration file for the current folder
                
                [green bold]Start[/]
                Creates and starts a new Task
                
                [green bold]End[/]
                Ends a Task
                
                [green bold]Work[/]
                Active Time Tracking for a specific Task - Focus Work
                
                [green bold]Details[/]
                Shows details of a Task
                
                [green bold]List[/]
                Lists all Tasks
                
                [green bold]Developer[/]
                Creates and edit information about the developer
                
                [green bold]Info[/]
                Shows information about the current version of DotTimeWork
                
                [green bold]Report[/]
                Creates a report of the current project in CSV or HTML format
                
                [green bold]Comment[/]
                Add a comment to a Task
                """;
        }
    }
}
