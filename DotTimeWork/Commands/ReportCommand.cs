using DotTimeWork.Helper;
using DotTimeWork.Project;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace DotTimeWork.Commands
{
    internal class ReportCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IProjectConfigController _projectConfigController;

        private const string HTML_CSS_STYLE =
            "table { border-collapse: collapse; width: 100%; }" +
            "th, td { border: 1px solid black; padding: 8px; text-align: left; }" +
            "th { background-color: #f2f2f2; }";

        public ReportCommand(ITaskTimeTracker taskTimeTracker,IProjectConfigController projectConfigController) : base("Report", "Generate a report of all active and completed Tasks")
        {
            _projectConfigController = projectConfigController;
            _taskTimeTracker = taskTimeTracker;
            var generateCsvSubCommand = new Command("csv", "Generate a CSV report of all active and completed Tasks");
            generateCsvSubCommand.AddOption(PublicOptions.OutputFile);
            generateCsvSubCommand.AddOption(PublicOptions.OpenReportAfterCreate);
            generateCsvSubCommand.AddOption(PublicOptions.ReportIncludeFinishedTasks);
            generateCsvSubCommand.SetHandler(ExecuteCsvReport, PublicOptions.OutputFile, PublicOptions.OpenReportAfterCreate, PublicOptions.VerboseLogging);
            AddCommand(generateCsvSubCommand);
            var generateHtmlSubCommand = new Command("html", "Generate a HTML report of all active and completed Tasks");
            generateHtmlSubCommand.AddOption(PublicOptions.OutputFile);
            generateHtmlSubCommand.AddOption(PublicOptions.OpenReportAfterCreate);
            generateHtmlSubCommand.AddOption(PublicOptions.ReportIncludeFinishedTasks);
            generateHtmlSubCommand.SetHandler(ExecuteHtmlReport, PublicOptions.OutputFile, PublicOptions.ReportIncludeFinishedTasks, PublicOptions.OpenReportAfterCreate, PublicOptions.VerboseLogging);
            AddCommand(generateHtmlSubCommand);
            this.SetHandler(ExecuteReportGeneration);
        }

        private void ExecuteReportGeneration(InvocationContext context)
        {
            AnsiConsole.WriteLine("Please specify the report format: csv or html");
        }

        private void ExecuteHtmlReport(string outputFile, bool includeFinishedTasks, bool openAfterCreate, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = Path.Combine(Environment.CurrentDirectory, $"report_{GetCurrentDayString()}.html");
            }

            AnsiConsole.MarkupLine($"[green]Started generating HTML report at:[/] {outputFile}");

            using (var writer = new StreamWriter(outputFile))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine($"<title>{GlobalConstants.REPORT_HTML_TITLE}</title>");
                writer.WriteLine("<style>" + HTML_CSS_STYLE+ "</style>");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
                writer.WriteLine($"<h1>{GlobalConstants.REPORT_HTML_TITLE}</h1>");
                writer.WriteLine("<p><strong>Generated on</strong>: " + GetCurrentDayString() + "</p>");
                
                GenerateInfoSection(writer);

                GenerateTableActiveTask(writer);

                var allFinishedTasks = _taskTimeTracker.GetAllFinishedTasks();
                bool anyFinishedTasks = allFinishedTasks.Any();
                if(includeFinishedTasks && !anyFinishedTasks && verboseLogging)
                {
                    AnsiConsole.MarkupLine("[red]Requested was to include finished Tasks but no finished tasks found.[/]");
                }
                if (includeFinishedTasks && anyFinishedTasks)
                {
                    GenerateTableFinishedTask(writer);
                }
                writer.WriteLine("<p /><hr /><p>Dot Time Worker tool developed by Carl-Philip Wenz</p>");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }

            if (verboseLogging)
            {
                AnsiConsole.MarkupLine($"[grey]HTML report generation finished[/]");
                if(openAfterCreate)
                {
                    AnsiConsole.MarkupLine($"[grey]Opening HTML report...[/]");
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

        private void GenerateInfoSection(StreamWriter writer)
        {
            writer.WriteLine($"<h2>Project</h2>");
            var projectConfig=_projectConfigController.GetCurrentProjectConfig();
            writer.WriteLine("<ul>");
            writer.WriteLine($"<li>Project Name: {projectConfig.ProjectName}</li>");
            writer.WriteLine($"<li>Project Description: {projectConfig.Description}</li>");
            writer.WriteLine($"<li>Project Start Date: {projectConfig.ProjectStart}</li>");
            writer.WriteLine($"<li>Project Working Time: {projectConfig.MaxTimePerDay}</li>");
            writer.WriteLine("</ul>");
        }

        private void GenerateTableActiveTask(StreamWriter writer)
        {
            writer.WriteLine("<h2>Active Tasks</h2>");
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>Task Name</th><th>Working time</th><th>Focus Time</th><th>Developer</th></tr>");
            DateTime dateTime = DateTime.Now;
            // Aufgaben in die Tabelle einfügen
            foreach (var task in _taskTimeTracker.GetAllRunningTasks())
            {
                writer.WriteLine($"<tr><td>{task.Name}</td><td>{TimeHelper.GetWorkingTimeHumanReadable(dateTime - task.Started)}</td><td>{task.FocusWorkTime}</td><td>{task.Developer}</td></tr>");
            }

            writer.WriteLine("</table>");
        }

        private void GenerateTableFinishedTask(StreamWriter writer)
        {
            writer.WriteLine("<h2>Completed Tasks</h2>");
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>Task Name</th><th>Focus Time</th><th>Developer</th></tr>");
            // Aufgaben in die Tabelle einfügen
            foreach (var task in _taskTimeTracker.GetAllFinishedTasks())
            {
                writer.WriteLine($"<tr><td>{task.Name}</td><td>{task.FocusWorkTime}</td><td>{task.Developer}</td></tr>");
            }

            writer.WriteLine("</table>");
        }

        private void ExecuteCsvReport(string outputFile, bool openAfterCreate, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = Path.Combine(Environment.CurrentDirectory, $"report_{GetCurrentDayString()}.csv");
            }
            throw new NotImplementedException();
        }

        private static string GetCurrentDayString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
