using DotTimeWork.Commands.Base;
using DotTimeWork.Commands.Report;
using DotTimeWork.Helper;
using DotTimeWork.Project;
using DotTimeWork.Services;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace DotTimeWork.Commands
{
    internal class ReportCommand : BaseCommand
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IProjectConfigController _projectConfigController;
        private readonly ITotalWorkingTimeCalculator _totalWorkingTimeCalculator;

        public ReportCommand(
            ITaskTimeTracker taskTimeTracker,
            IProjectConfigController projectConfigController,
            ITotalWorkingTimeCalculator totalWorkingTimeCalculator) 
            : base("Report", "Generate a report of all active and completed Tasks")
        {
            _taskTimeTracker = taskTimeTracker ?? throw new ArgumentNullException(nameof(taskTimeTracker));
            _projectConfigController = projectConfigController ?? throw new ArgumentNullException(nameof(projectConfigController));
            _totalWorkingTimeCalculator = totalWorkingTimeCalculator ?? throw new ArgumentNullException(nameof(totalWorkingTimeCalculator));
        }

        protected override void SetupCommand()
        {
            SetupCsvSubCommand();
            SetupHtmlSubCommand();
            this.SetHandler(ExecuteReportGeneration);
        }

        private void SetupCsvSubCommand()
        {
            var csvCommand = new Command("csv", "Generate a CSV report of all active and completed Tasks");
            csvCommand.AddOption(PublicOptions.OutputFile);
            csvCommand.AddOption(PublicOptions.OpenReportAfterCreate);
            csvCommand.AddOption(PublicOptions.ReportIncludeFinishedTasks);
            csvCommand.AddOption(PublicOptions.ReportIncludeComments);
            csvCommand.SetHandler(ExecuteCsvReport, 
                PublicOptions.OutputFile, 
                PublicOptions.OpenReportAfterCreate, 
                PublicOptions.VerboseLogging);
            AddCommand(csvCommand);
        }

        private void SetupHtmlSubCommand()
        {
            var htmlCommand = new Command("html", "Generate an HTML report of all active and completed Tasks");
            htmlCommand.AddOption(PublicOptions.OutputFile);
            htmlCommand.AddOption(PublicOptions.OpenReportAfterCreate);
            htmlCommand.AddOption(PublicOptions.ReportIncludeFinishedTasks);
            htmlCommand.AddOption(PublicOptions.ReportIncludeComments);
            htmlCommand.SetHandler(ExecuteHtmlReport,
                PublicOptions.OutputFile,
                PublicOptions.ReportIncludeFinishedTasks,
                PublicOptions.ReportIncludeComments,
                PublicOptions.OpenReportAfterCreate,
                PublicOptions.VerboseLogging);
            AddCommand(htmlCommand);
        }

        private void ExecuteReportGeneration(InvocationContext context)
        {
            Console.PrintInfo("Please specify the report format: csv or html");
            Console.PrintInfo("Examples:");
            Console.PrintInfo("  dottimework report csv");
            Console.PrintInfo("  dottimework report html --output myreport.html");
        }

        public void ExecuteCsvReport(string? outputFile, bool openAfterCreate, bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                var resolvedOutputFile = ResolveOutputFilePath(outputFile, "csv");
                
                Console.PrintInfo($"Started generating CSV report at: {resolvedOutputFile}");

                var csvExport = new CsvExport(_taskTimeTracker);
                var success = csvExport.ExecuteCsvReport(resolvedOutputFile);

                HandleReportResult(success, resolvedOutputFile, "CSV", openAfterCreate, verboseLogging);
            }, verboseLogging);
        }

        public void ExecuteHtmlReport(string? outputFile, bool includeFinishedTasks, bool includeComments, bool openAfterCreate, bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                var resolvedOutputFile = ResolveOutputFilePath(outputFile, "html");
                
                Console.PrintInfo($"Started generating HTML report at: {resolvedOutputFile}");

                var projectConfig = GetProjectConfigSafely();
                if (projectConfig == null)
                {
                    Console.PrintError("Could not load project configuration. HTML report generation requires a valid project.");
                    return;
                }

                var htmlExport = new HtmlExport(_taskTimeTracker, projectConfig, _totalWorkingTimeCalculator);
                
                var success = htmlExport.ExecuteHtmlReport(resolvedOutputFile, includeFinishedTasks, includeComments, verboseLogging);

                HandleReportResult(success, resolvedOutputFile, "HTML", openAfterCreate, verboseLogging);
            }, verboseLogging);
        }

        private string ResolveOutputFilePath(string? providedPath, string format)
        {
            if (!string.IsNullOrWhiteSpace(providedPath))
            {
                return providedPath;
            }

            var fileName = $"report_{TimeHelper.GetCurrentDayString()}.{format}";
            return Path.Combine(Environment.CurrentDirectory, fileName);
        }

        private ProjectConfig? GetProjectConfigSafely()
        {
            try
            {
                return _projectConfigController.GetCurrentProjectConfig();
            }
            catch (Exception ex)
            {
                if (PublicOptions.IsVerbosLogging)
                {
                    Console.PrintWarning($"Could not load project config: {ex.Message}");
                }
                return null;
            }
        }

        private void HandleReportResult(bool success, string outputFile, string reportType, bool openAfterCreate, bool verboseLogging)
        {
            if (success)
            {
                Console.PrintSuccess($"{reportType} report generated successfully!");
                
                if (verboseLogging)
                {
                    Console.PrintDebug($"{reportType} report generation finished");
                }

                if (openAfterCreate)
                {
                    OpenReportFile(outputFile, verboseLogging);
                }
            }
            else
            {
                Console.PrintError($"Error generating {reportType} report.");
            }
        }

        private void OpenReportFile(string filePath, bool verboseLogging)
        {
            try
            {
                if (verboseLogging)
                {
                    Console.PrintDebug($"Opening report file: {filePath}");
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });

                if (verboseLogging)
                {
                    Console.PrintDebug("Report file opened successfully");
                }
            }
            catch (Exception ex)
            {
                Console.PrintError($"Failed to open report file: {ex.Message}");
            }
        }
    }
}
