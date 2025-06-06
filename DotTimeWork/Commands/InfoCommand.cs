﻿using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTimeWork.Commands
{
    internal class InfoCommand : Command
    {
        public InfoCommand() : base("Info", Properties.Resources.Info_Description)
        {
            this.SetHandler(Execute);
        }

        private void Execute()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                    new FigletText($"DotTimeWork Version {GlobalConstants.RELEASE_VERSION}")
                        .Centered()
                            .Color(Color.Yellow));
            AnsiConsole.MarkupLine(Properties.Resources.Info_FirstLine);
            AnsiConsole.MarkupLine(Properties.Resources.Info_SecondLine);
            AnsiConsole.MarkupLine(Properties.Resources.Info_Development);
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            string includedFunctions =
                """
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



            Panel panel = new Panel(new Markup(includedFunctions))
            {
                Border = BoxBorder.Heavy,
                Header = new PanelHeader("Included functions"),
                Expand = true,
                Padding = new Padding(2, 2, 2, 2),
                Width = 80,
            };
            AnsiConsole.Write(new Padder(panel).PadRight(5).PadLeft(5));
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            var rule2 = new Rule("[red]Development[/]");
            AnsiConsole.Write(rule2);
            AnsiConsole.MarkupLine("[green]Developer: [/][bold]Carl-Philip Wenz[/]");
            AnsiConsole.MarkupLine("[gray]Statistics (Count of registered DI Services): [/]"+DI.CountOfDependencies);
            AnsiConsole.MarkupLine("[gray]Development just started, so please be patient.[/]");
            AnsiConsole.WriteLine();


        }
    }
}
