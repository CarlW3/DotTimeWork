using DotTimeWork.Commands.Report;
using DotTimeWork.Helper;
using DotTimeWork.Project;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Text;

namespace DotTimeWork.Commands
{
    internal class ReportCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IProjectConfigController _projectConfigController;

        public ReportCommand(ITaskTimeTracker taskTimeTracker,IProjectConfigController projectConfigController) : base("Report", "Generate a report of all active and completed Tasks")
        {
            _projectConfigController = projectConfigController;
            _taskTimeTracker = taskTimeTracker;
            var generateCsvSubCommand = new Command("csv", "Generate a CSV report of all active and completed Tasks");
            generateCsvSubCommand.AddOption(PublicOptions.OutputFile);
            generateCsvSubCommand.AddOption(PublicOptions.OpenReportAfterCreate);
            generateCsvSubCommand.AddOption(PublicOptions.ReportIncludeFinishedTasks);
            generateCsvSubCommand.AddOption(PublicOptions.ReportIncludeComments);
            generateCsvSubCommand.SetHandler(ExecuteCsvReport, PublicOptions.OutputFile, PublicOptions.OpenReportAfterCreate, PublicOptions.VerboseLogging);
            AddCommand(generateCsvSubCommand);
            var generateHtmlSubCommand = new Command("html", "Generate a HTML report of all active and completed Tasks");
            generateHtmlSubCommand.AddOption(PublicOptions.OutputFile);
            generateHtmlSubCommand.AddOption(PublicOptions.OpenReportAfterCreate);
            generateHtmlSubCommand.AddOption(PublicOptions.ReportIncludeFinishedTasks);
            generateHtmlSubCommand.AddOption(PublicOptions.ReportIncludeComments);
            generateHtmlSubCommand.SetHandler(ExecuteHtmlReport, PublicOptions.OutputFile, PublicOptions.ReportIncludeFinishedTasks, PublicOptions.ReportIncludeComments, PublicOptions.OpenReportAfterCreate, PublicOptions.VerboseLogging);
            AddCommand(generateHtmlSubCommand);
            this.SetHandler(ExecuteReportGeneration);
        }

        private void ExecuteReportGeneration(InvocationContext context)
        {
            AnsiConsole.WriteLine("Please specify the report format: csv or html");
        }

        private void ExecuteHtmlReport(string outputFile, bool includeFinishedTasks, bool includeComments, bool openAfterCreate, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = Path.Combine(Environment.CurrentDirectory, $"report_{TimeHelper.GetCurrentDayString()}.html");
            }

            AnsiConsole.MarkupLine($"[green]Started generating HTML report at:[/] {outputFile}");

            HtmlExport htmlExport = new HtmlExport(_taskTimeTracker, _projectConfigController.GetCurrentProjectConfig());
            if (htmlExport.ExecuteHtmlReport(outputFile, includeFinishedTasks, includeComments, verboseLogging))
            {
                if (verboseLogging)
                {
                    AnsiConsole.MarkupLine($"[grey]HTML report generation finished[/]");
                    if (openAfterCreate)
                    {
                        AnsiConsole.MarkupLine($"[grey]Opening HTML report...[/]");
                    }
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Error generating HTML report.[/]");
            }

            if (openAfterCreate)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = outputFile,
                    UseShellExecute = true
                });
            }
        }

        private void ExecuteCsvReport(string outputFile, bool openAfterCreate, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = Path.Combine(Environment.CurrentDirectory, $"report_{TimeHelper.GetCurrentDayString()}.csv");
            }
            CsvExport csvExport = new CsvExport(_taskTimeTracker);
            if (csvExport.ExecuteCsvReport(outputFile))
            {
                if (verboseLogging)
                {
                    AnsiConsole.MarkupLine($"[grey]CSV report generation finished[/]");
                    if (openAfterCreate)
                    {
                        AnsiConsole.MarkupLine($"[grey]Opening CSV report...[/]");
                    }
                }
                if (openAfterCreate)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = outputFile,
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Error generating CSV report.[/]");
            }
        }
    }
}
