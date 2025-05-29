using DotTimeWork.Helper;
using DotTimeWork.Project;
using DotTimeWork.Services;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.Text;

namespace DotTimeWork.Commands.Report
{
    internal class HtmlExport
    {
        private const string HTML_CSS_STYLE =
            "table { border-collapse: collapse; width: 100%; }" +
            "th, td { border: 1px solid black; padding: 8px; text-align: left; }" +
            "th { background-color: #f2f2f2; }";

        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly ITotalWorkingTimeCalculator _totalWorkingTimeCalculator;
        private readonly ProjectConfig _projectConfig;

        public HtmlExport(ITaskTimeTracker taskTimeTracker, ProjectConfig projectConfig, ITotalWorkingTimeCalculator totalWorkingTimeCalculator)
        {
            _taskTimeTracker = taskTimeTracker;
            _projectConfig = projectConfig;
            _totalWorkingTimeCalculator = totalWorkingTimeCalculator;
        }

        public bool ExecuteHtmlReport(string outputFile, bool includeFinishedTasks, bool includeComments, bool verboseLogging)
        {
            TimeSpan activeTasksWorkingTime;
            TimeSpan finishedTasksWorkingTime;
            int focusWorkingTime;
            try
            {
                _totalWorkingTimeCalculator.LoadTasks();
                activeTasksWorkingTime = _totalWorkingTimeCalculator.GetTotalTimeRunningTasks();
                finishedTasksWorkingTime = _totalWorkingTimeCalculator.GetTotalTimeFinishedTasks();
                focusWorkingTime = _totalWorkingTimeCalculator.TotalMinutesFocusWorkingTime;
            }
            catch
            {
                Console.Error.WriteLine("Error calculating total working time. Please check your task data.");
                return false;
            }
            try
            {
                using (var writer = new StreamWriter(outputFile))
                {
                    writer.WriteLine("<!DOCTYPE html>");
                    writer.WriteLine("<html>");
                    writer.WriteLine("<head>");
                    writer.WriteLine($"<title>{GlobalConstants.REPORT_HTML_TITLE}</title>");
                    writer.WriteLine("<style>" + HTML_CSS_STYLE + "</style>");
                    writer.WriteLine("</head>");
                    writer.WriteLine("<body>");
                    writer.WriteLine($"<h1>{GlobalConstants.REPORT_HTML_TITLE}</h1>");
                    writer.WriteLine("<p><strong>Generated on</strong>: " + TimeHelper.GetCurrentDayTimeString() + "</p>");

                    GenerateInfoSection(writer, activeTasksWorkingTime, finishedTasksWorkingTime, focusWorkingTime);

                    GenerateTableActiveTask(writer, includeComments);

                    var allFinishedTasks = _taskTimeTracker.GetAllFinishedTasks();
                    bool anyFinishedTasks = allFinishedTasks.Any();
                    if (includeFinishedTasks && !anyFinishedTasks && verboseLogging)
                    {
                        AnsiConsole.MarkupLine("[red]Requested was to include finished Tasks but no finished tasks found.[/]");
                    }
                    if (includeFinishedTasks && anyFinishedTasks)
                    {
                        GenerateTableFinishedTask(writer, includeComments);
                    }
                    writer.WriteLine("<p /><hr /><p>Dot Time Worker tool developed by Carl-Philip Wenz</p>");
                    writer.WriteLine("</body>");
                    writer.WriteLine("</html>");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error generating HTML report: {ex.Message}");
                return false;
            }
        }

        private void GenerateInfoSection(StreamWriter writer,TimeSpan activeWorking,TimeSpan finishedWorkingTime, int focusWokringTime)
        {
            TimeSpan sum= activeWorking + finishedWorkingTime;
            writer.WriteLine($"<h2>Project</h2>");
            writer.WriteLine("<ul>");
            writer.WriteLine($"<li>Project Name: {_projectConfig.ProjectName}</li>");
            writer.WriteLine($"<li>Project Description: {_projectConfig.Description}</li>");
            writer.WriteLine($"<li>Project Start Date: {_projectConfig.ProjectStart}</li>");
            writer.WriteLine($"<li>Planned project Working Time per day: {_projectConfig.MaxTimePerDay}</li>");
            writer.WriteLine($"<li>Total executed working Time: {TimeHelper.GetWorkingTimeHumanReadable(sum)}"
                + $"<ul>"
                + $"<li>Active Task Working Time</li>" + $"{TimeHelper.GetWorkingTimeHumanReadable(activeWorking)}" + $"<li>Finished Task Working Time</li>" + $"{TimeHelper.GetWorkingTimeHumanReadable(finishedWorkingTime)}" +
                "</ul>"
                + "</li>");
            writer.WriteLine($"<li>Total Focus Working Time: {TimeHelper.GetWorkingTimeHumanReadable(focusWokringTime)}</li>");
            writer.WriteLine("</ul>");
        }

        private void GenerateTableActiveTask(StreamWriter writer, bool includeComments)
        {
            writer.WriteLine("<h2>Active Tasks</h2>");
            writer.WriteLine("<table>");
            string commentHeader = includeComments ? "<th>Comments</th>" : "";
            writer.WriteLine($"<tr><th>Task Name</th><th>Working time</th><th>Focus Time</th><th>Developer</th>{commentHeader}</tr>");
            DateTime dateTime = DateTime.Now;
            // Aufgaben in die Tabelle einfügen
            foreach (var task in _taskTimeTracker.GetAllRunningTasks())
            {
                string commentColumn = includeComments ? $"<td>{GetComments(task)}</td>" : "";
                writer.WriteLine($"<tr><td>{task.Name}</td><td>{TimeHelper.GetWorkingTimeHumanReadable(dateTime - task.Started)}</td><td>{task.FocusWorkTime}</td><td>{task.Developer}</td>{commentColumn}</tr>");
            }

            writer.WriteLine("</table>");
        }

        private static string GetComments(TaskData task)
        {
            StringBuilder comments = new StringBuilder();
            if (task.Comments != null && task.Comments.Count > 0)
            {
                comments.AppendLine("<ul>");
                foreach (var comment in task.Comments)
                {
                    comments.AppendLine($"<li>{comment.Created.ToString()} | {comment.Developer} | {comment.Comment}</li>");
                }
                comments.AppendLine("</ul>");
            }
            else
            {
                comments.AppendLine("No comments");
            }
            return comments.ToString().Trim();
        }

        private void GenerateTableFinishedTask(StreamWriter writer, bool includeComments)
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

    }
}
