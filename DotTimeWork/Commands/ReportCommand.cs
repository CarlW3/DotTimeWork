using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace DotTimeWork.Commands
{
    internal class ReportCommand:Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;

        public ReportCommand(ITaskTimeTracker taskTimeTracker) : base("Report", "Generate a report of all active and completed Tasks")
        {
            _taskTimeTracker = taskTimeTracker;
            var generateCsvSubCommand=new Command("csv", "Generate a CSV report of all active and completed Tasks");
            generateCsvSubCommand.AddOption(PublicOptions.OutputFile);
            generateCsvSubCommand.AddOption(PublicOptions.OpenReportAfterCreate);
            generateCsvSubCommand.SetHandler(ExecuteCsvReport, PublicOptions.OutputFile, PublicOptions.OpenReportAfterCreate,PublicOptions.VerboseLogging);
            AddCommand(generateCsvSubCommand);
            var generateHtmlSubCommand = new Command("html", "Generate a HTML report of all active and completed Tasks");
            generateHtmlSubCommand.AddOption(PublicOptions.OutputFile);
            generateHtmlSubCommand.AddOption(PublicOptions.OpenReportAfterCreate);
            generateHtmlSubCommand.SetHandler(ExecuteHtmlReport, PublicOptions.OutputFile, PublicOptions.OpenReportAfterCreate, PublicOptions.VerboseLogging);
            AddCommand(generateHtmlSubCommand);
            this.SetHandler(ExecuteReportGeneration);
        }

        private void ExecuteReportGeneration(InvocationContext context)
        {
            AnsiConsole.WriteLine("Please specify the report format: csv or html");
        }

        private void ExecuteHtmlReport(string outputFile, bool openAfterCreate, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = Path.Combine(Environment.CurrentDirectory, $"report_{GetCurrentDayString()}.html");
            }

            AnsiConsole.WriteLine($"Generating HTML report at: {outputFile}");

            using (var writer = new StreamWriter(outputFile))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine("<title>Dot Time Worker Report</title>");
                writer.WriteLine("<style>");
                writer.WriteLine("table { border-collapse: collapse; width: 100%; }");
                writer.WriteLine("th, td { border: 1px solid black; padding: 8px; text-align: left; }");
                writer.WriteLine("th { background-color: #f2f2f2; }");
                writer.WriteLine("</style>");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
                writer.WriteLine("<h1>Dot Time Worker Report</h1>");
                writer.WriteLine("<p><strong>Generated on</strong>: " + GetCurrentDayString() + "</p>");

                writer.WriteLine("<h2>Active Tasks</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Task Name</th><th>Working time</th><th>Focus Time</th><th>Developer</th></tr>");
                DateTime dateTime = DateTime.Now;
                // Aufgaben in die Tabelle einfügen
                foreach (var task in _taskTimeTracker.GetAllTasks())
                {
                    writer.WriteLine($"<tr><td>{task.Name}</td><td>{TimeHelper.GetWorkingTimeHumanReadable(dateTime-task.Started)}</td><td>{task.FocusWorkTime}</td><td>{task.Developer}</td></tr>");
                }

                writer.WriteLine("</table>");

                var allFinishedTasks = _taskTimeTracker.GetAllFinishedTasks();
                if (allFinishedTasks.Any())
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
                writer.WriteLine("<p /><hr /><p>Dot Time Worker tool developed by Carl-Philip Wenz</p>");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }

            if (verboseLogging)
            {
                Console.WriteLine($"HTML report generated at: {outputFile}");
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
