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
        // Path to the CSS file for HTML export styling
        private const string CssFilePath = "Commands/Report/HtmlExportStyle.css";

        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly ITotalWorkingTimeCalculator _totalWorkingTimeCalculator;
        private readonly ProjectConfig _projectConfig;

        public HtmlExport(ITaskTimeTracker taskTimeTracker, ProjectConfig projectConfig, ITotalWorkingTimeCalculator totalWorkingTimeCalculator)
        {
            _taskTimeTracker = taskTimeTracker;
            _projectConfig = projectConfig;
            _totalWorkingTimeCalculator = totalWorkingTimeCalculator;
        }

        /// <summary>
        /// Loads the CSS style from the external file.
        /// </summary>
        private static string LoadCss()
        {
            var cssPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CssFilePath.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(cssPath) ? File.ReadAllText(cssPath) : string.Empty;
        }

        /// <summary>
        /// Generates the HTML report and writes it to the specified output file.
        /// </summary>
        public bool ExecuteHtmlReport(string outputFile, bool includeFinishedTasks, bool includeComments, bool verboseLogging)
        {
            try
            {
                _totalWorkingTimeCalculator.LoadTasks();

                var activeTasks = _taskTimeTracker.GetAllRunningTasks();
                var finishedTasks = _taskTimeTracker.GetAllFinishedTasks();

                TimeSpan activeTasksWorkingTime = _totalWorkingTimeCalculator.GetTotalTimeRunningTasks();
                TimeSpan finishedTasksWorkingTime = _totalWorkingTimeCalculator.GetTotalTimeFinishedTasks();
                int focusWorkingTime = _totalWorkingTimeCalculator.TotalMinutesFocusWorkingTime;

                using (var writer = new StreamWriter(outputFile))
                {
                    WriteHtmlHeader(writer);
                    WriteProjectOverview(writer, activeTasks, finishedTasks, activeTasksWorkingTime, finishedTasksWorkingTime, focusWorkingTime);
                    WriteActiveTasksTable(writer, activeTasks, includeComments);
                    if (includeFinishedTasks)
                    {
                        if (!finishedTasks.Any() && verboseLogging)
                        {
                            AnsiConsole.MarkupLine("[red]Requested was to include finished Tasks but no finished tasks found.[/]");
                        }
                        else if (finishedTasks.Any())
                        {
                            WriteFinishedTasksTable(writer, finishedTasks, includeComments);
                        }
                    }
                    WriteStatisticsSection(writer, activeTasksWorkingTime, finishedTasksWorkingTime, focusWorkingTime);
                    WriteHtmlFooter(writer);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fehler beim Generieren des HTML-Reports: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Writes the HTML header, including CSS and JS includes.
        /// </summary>
        private static void WriteHtmlHeader(StreamWriter writer)
        {
            writer.WriteLine("<!DOCTYPE html>");
            writer.WriteLine("<html>");
            writer.WriteLine("<head>");
            writer.WriteLine($"<title>{GlobalConstants.REPORT_HTML_TITLE}</title>");
            writer.WriteLine("<meta charset=\"utf-8\" />");
            writer.WriteLine("<style>" + LoadCss() + "</style>");
            writer.WriteLine("<script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
            writer.WriteLine("</head>");
            writer.WriteLine("<body>");
            writer.WriteLine($"<h1>{GlobalConstants.REPORT_HTML_TITLE}</h1>");
            writer.WriteLine($"<p><strong>Generiert am:</strong> {TimeHelper.GetCurrentDayTimeString()}</p>");
        }

        /// <summary>
        /// Writes the project overview section.
        /// </summary>
        private void WriteProjectOverview(StreamWriter writer, List<TaskData> activeTasks, List<TaskData> finishedTasks, TimeSpan activeWorking, TimeSpan finishedWorking, int focusWorkingTime)
        {
            TimeSpan totalWorking = activeWorking + finishedWorking;
            int totalTasks = activeTasks.Count + finishedTasks.Count;
            double avgFocus = totalTasks > 0 ? (double)focusWorkingTime / totalTasks : 0;
            int plannedPerDay = _projectConfig.MaxTimePerDay;
            int plannedTotal = plannedPerDay * (int)(DateTime.Now - _projectConfig.ProjectStart).TotalDays;
            int actualTotal = (int)totalWorking.TotalMinutes;

            writer.WriteLine("<h2>Projektübersicht</h2>");
            writer.WriteLine("<ul>");
            writer.WriteLine($"<li><span class=\"stat\">Projektname:</span> {_projectConfig.ProjectName}</li>");
            writer.WriteLine($"<li><span class=\"stat\">Beschreibung:</span> {_projectConfig.Description}</li>");
            writer.WriteLine($"<li><span class=\"stat\">Projektstart:</span> {_projectConfig.ProjectStart:yyyy-MM-dd}</li>");
            if (_projectConfig.ProjectEnd.HasValue)
                writer.WriteLine($"<li><span class=\"stat\">Projektende:</span> {_projectConfig.ProjectEnd:yyyy-MM-dd}</li>");
            writer.WriteLine($"<li><span class=\"stat\">Gesamt geplante Arbeitszeit:</span> {TimeHelper.GetWorkingTimeHumanReadable(plannedTotal)}</li>");
            writer.WriteLine($"<li><span class=\"stat\">Tatsächliche Arbeitszeit:</span> {TimeHelper.GetWorkingTimeHumanReadable(actualTotal)}</li>");
            writer.WriteLine($"<li><span class=\"stat\">Aktive Aufgaben:</span> {activeTasks.Count}</li>");
            writer.WriteLine($"<li><span class=\"stat\">Abgeschlossene Aufgaben:</span> {finishedTasks.Count}</li>");
            writer.WriteLine($"<li><span class=\"stat\">Durchschnittliche Fokuszeit pro Aufgabe:</span> {TimeHelper.GetWorkingTimeHumanReadable((int)avgFocus)}</li>");
            writer.WriteLine($"<li><span class=\"stat\">Gesamte Fokuszeit:</span> {TimeHelper.GetWorkingTimeHumanReadable(focusWorkingTime)}</li>");
            writer.WriteLine("</ul>");
        }

        /// <summary>
        /// Writes the table of active tasks.
        /// </summary>
        private static void WriteActiveTasksTable(StreamWriter writer, List<TaskData> activeTasks, bool includeComments)
        {
            writer.WriteLine("<h2>Aktive Aufgaben</h2>");
            if (!activeTasks.Any())
            {
                writer.WriteLine("<p class=\"warn\">Keine aktiven Aufgaben vorhanden.</p>");
                return;
            }
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>Aufgabe</th><th>Arbeitszeit</th><th>Fokuszeit(en) pro Entwickler</th><th>Aktive Entwickler</th>" +
                (includeComments ? "<th>Kommentare</th>" : "") + "</tr>");
            DateTime now = DateTime.Now;
            foreach (var task in activeTasks)
            {
                string commentColumn = includeComments ? $"<td>{GetComments(task)}</td>" : "";
                string devTimes = string.Join("<br>", task.DeveloperWorkTimes.Select(kvp => $"{kvp.Key}: {TimeHelper.GetWorkingTimeHumanReadable(kvp.Value)}"));
                
                // Format active developers list, or show message if empty
                string activeDevelopers = task.ActiveDevelopers != null && task.ActiveDevelopers.Any() 
                    ? string.Join("<br>", task.ActiveDevelopers)
                    : "<em>Keine aktiven Entwickler</em>";
                    
                writer.WriteLine($"<tr><td>{task.Name}</td><td>{TimeHelper.GetWorkingTimeHumanReadable((int)(now - task.Started).TotalMinutes)}</td><td>{devTimes}</td><td>{activeDevelopers}</td>{commentColumn}</tr>");
            }
            writer.WriteLine("</table>");
        }

        /// <summary>
        /// Writes the table of finished tasks.
        /// </summary>
        private void WriteFinishedTasksTable(StreamWriter writer, List<TaskData> finishedTasks, bool includeComments)
        {
            writer.WriteLine("<h2>Abgeschlossene Aufgaben</h2>");
            if (!finishedTasks.Any())
            {
                writer.WriteLine("<p class=\"warn\">Keine abgeschlossenen Aufgaben vorhanden.</p>");
                return;
            }
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>Aufgabe</th><th>Fokuszeit(en) pro Entwickler</th><th>Abgeschlossen von</th><th>Abgeschlossen am</th>" +
                (includeComments ? "<th>Kommentare</th>" : "") + "</tr>");
            foreach (var task in finishedTasks)
            {
                string commentColumn = includeComments ? $"<td>{GetComments(task)}</td>" : "";
                string devTimes = string.Join("<br>", task.DeveloperWorkTimes.Select(kvp => $"{kvp.Key}: {TimeHelper.GetWorkingTimeHumanReadable(kvp.Value)}"));
                
                // Show who finished the task, or Unknown if not available
                string finishedBy = !string.IsNullOrEmpty(task.FinishedBy) 
                    ? task.FinishedBy 
                    : "<em>Unbekannt</em>";
                    
                writer.WriteLine($"<tr><td>{task.Name}</td><td>{devTimes}</td><td>{finishedBy}</td><td>{task.Finished:yyyy-MM-dd HH:mm}</td>{commentColumn}</tr>");
            }
            writer.WriteLine("</table>");
        }

        /// <summary>
        /// Writes the statistics section and chart.
        /// </summary>
        private static void WriteStatisticsSection(StreamWriter writer, TimeSpan activeTasksWorkingTime, TimeSpan finishedTasksWorkingTime, int focusWorkingTime)
        {
            writer.WriteLine("<h2>Arbeitszeit-Statistik</h2>");
            writer.WriteLine("<canvas id=\"workChart\" width=\"400\" height=\"100\"></canvas>");
            WriteChartJs(writer, activeTasksWorkingTime, finishedTasksWorkingTime, focusWorkingTime);
        }

        /// <summary>
        /// Writes the HTML footer.
        /// </summary>
        private static void WriteHtmlFooter(StreamWriter writer)
        {
            writer.WriteLine("<hr /><p class=\"footer\">Dot Time Worker tool entwickelt von Carl-Philip Wenz</p>");
            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
        }

        private static string GetComments(TaskData task)
        {
            if (task.Comments != null && task.Comments.Count > 0)
            {
                var sb = new StringBuilder("<ul>");
                foreach (var comment in task.Comments)
                {
                    sb.AppendLine($"<li>{comment.Created:yyyy-MM-dd HH:mm} | {comment.Developer} | {comment.Comment}</li>");
                }
                sb.AppendLine("</ul>");
                return sb.ToString();
            }
            return "Keine Kommentare";
        }

        private static void WriteChartJs(StreamWriter writer, TimeSpan active, TimeSpan finished, int focus)
        {
            bool minutesOnly = active.TotalMinutes < 60 && finished.TotalMinutes < 60 && focus < 60;
            writer.WriteLine(@"
<script>
const ctx = document.getElementById('workChart').getContext('2d');
new Chart(ctx, {
    type: 'bar',
    data: {
        labels: ['Aktive Aufgaben', 'Abgeschlossene Aufgaben', 'Fokuszeit'],
        datasets: [{
            label: '"+(minutesOnly?"Minuten":"Stunden")+@"',
            data: [
                " + (minutesOnly?(int)active.TotalMinutes:(int)active.TotalHours) + @",
                " + (minutesOnly ? (int)finished.TotalMinutes:(int)finished.TotalHours) + @",
                " + (minutesOnly ? focus:focus/60) + @"
            ],
            backgroundColor: [
                'rgba(42, 101, 146, 0.7)',
                'rgba(27, 126, 60, 0.7)',
                'rgba(184, 92, 0, 0.7)'
            ],
            borderColor: [
                'rgba(42, 101, 146, 1)',
                'rgba(27, 126, 60, 1)',
                'rgba(184, 92, 0, 1)'
            ],
            borderWidth: 1
        }]
    },
    options: {
        scales: {
            y: { beginAtZero: true }
        }
    }
});
</script>
");
        }
    }
}
