using DotTimeWork.Commands.Base;
using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;
using System.Text;

namespace DotTimeWork.Commands
{
    internal class DetailsTaskCommand : BaseCommand
    {
        private readonly ITaskTimeTracker _taskTimeTracker;

        public DetailsTaskCommand(ITaskTimeTracker taskTimeTracker) 
            : base("Details", Properties.Resources.Details_Description)
        {
            _taskTimeTracker = taskTimeTracker ?? throw new ArgumentNullException(nameof(taskTimeTracker));
        }

        protected override void SetupCommand()
        {
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.VerboseLogging);
        }

        public void Execute(string? taskId, bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                var selectedTaskId = GetTaskIdForDetails(taskId);
                if (string.IsNullOrEmpty(selectedTaskId))
                {
                    Console.PrintWarning("No task selected for details view.");
                    return;
                }

                var task = GetTaskDetails(selectedTaskId);
                if (task == null)
                {
                    Console.PrintError($"Task '{selectedTaskId}' not found.");
                    return;
                }

                DisplayTaskDetails(task, verboseLogging);
            }, verboseLogging);
        }

        private string? GetTaskIdForDetails(string? providedTaskId)
        {
            if (!string.IsNullOrWhiteSpace(providedTaskId))
            {
                Console.PrintInfo($"Showing details for task '{providedTaskId}'.");
                return providedTaskId;
            }

            var availableTasks = GetAvailableTaskNames();
            if (!availableTasks.Any())
            {
                Console.PrintWarning("No tasks available.");
                return null;
            }

            return Console.ShowTaskSelection(availableTasks, "Select [green]Task[/] for Details?");
        }

        private string[] GetAvailableTaskNames()
        {
            try
            {
                return _taskTimeTracker.GetAllRunningTasks()
                    .Select(x => x.Name)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.PrintError($"Error retrieving tasks: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        private TaskData? GetTaskDetails(string taskId)
        {
            try
            {
                return _taskTimeTracker.GetTaskById(taskId);
            }
            catch (Exception ex)
            {
                Console.PrintError($"Error retrieving task details: {ex.Message}");
                return null;
            }
        }

        private void DisplayTaskDetails(TaskData task, bool verboseLogging)
        {
            DisplayBasicTaskInfo(task);
            DisplayDeveloperTimes(task);
            DisplayTaskDescription(task);
            DisplayTaskComments(task);
            DisplayTaskTimeline(task, verboseLogging);
        }

        private void DisplayBasicTaskInfo(TaskData task)
        {
            Console.PrintMarkup($"[green]{Properties.Resources.List_Column_TaskName}:[/] {task.Name}");
            Console.PrintMarkup($"[green]Creation Time:[/] {task.Created:yyyy-MM-dd HH:mm:ss}");
        }

        private void DisplayDeveloperTimes(TaskData task)
        {
            if (!task.DeveloperWorkTimes.Any())
            {
                Console.PrintMarkup("[yellow]No focus time recorded yet[/]");
                return;
            }

            Console.PrintMarkup("[green]Developer(s) & Focus Time:[/]");
            foreach (var (developer, focusTime) in task.DeveloperWorkTimes)
            {
                var timeDisplay = TimeHelper.GetWorkingTimeHumanReadable(focusTime);
                Console.PrintMarkup($"  [yellow]{developer}[/]: {timeDisplay}");
            }

            var totalFocusTime = task.DeveloperWorkTimes.Values.Sum();
            var totalTimeDisplay = TimeHelper.GetWorkingTimeHumanReadable(totalFocusTime);
            Console.PrintMarkup($"[green]Total Focus Time:[/] {totalTimeDisplay}");
        }

        private void DisplayTaskDescription(TaskData task)
        {
            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                Console.PrintMarkup($"[green]Description:[/] {task.Description}");
            }
        }

        private void DisplayTaskComments(TaskData task)
        {
            if (task.Comments?.Any() != true)
            {
                return;
            }

            var commentsText = BuildCommentsText(task.Comments);
            var commentPanel = new Panel(new Markup(commentsText))
            {
                Header = new PanelHeader("Comments"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };
            
            AnsiConsole.Write(commentPanel);
        }

        private static string BuildCommentsText(IList<TaskComment> comments)
        {
            var commentsBuilder = new StringBuilder();
            foreach (var comment in comments.OrderBy(c => c.Created))
            {
                commentsBuilder.AppendLine($"[white]{comment.Created:yyyy-MM-dd HH:mm:ss}[/] | [yellow]{comment.Developer}[/] | {comment.Comment}");
            }
            return commentsBuilder.ToString().TrimEnd();
        }

        private void DisplayTaskTimeline(TaskData task, bool verboseLogging)
        {
            if (!task.DeveloperStartTimes.Any())
            {
                if (verboseLogging)
                {
                    Console.PrintDebug("No developer start times recorded");
                }
                return;
            }

            Console.PrintMarkup("[green]Developer Start Times & Working Duration:[/]");
            var now = DateTime.Now;
            
            foreach (var (developer, startTime) in task.DeveloperStartTimes.OrderBy(kvp => kvp.Value))
            {
                var workingDuration = (int)(now - startTime).TotalMinutes;
                var workingTimeDisplay = TimeHelper.GetWorkingTimeHumanReadable(workingDuration);
                
                Console.PrintMarkup($"  [yellow]{developer}[/]: Started {startTime:yyyy-MM-dd HH:mm:ss} - Working: {workingTimeDisplay}");
            }

            if (verboseLogging)
            {
                DisplayVerboseTaskInfo(task);
            }
        }

        private void DisplayVerboseTaskInfo(TaskData task)
        {
            Console.PrintDebug($"Task has {task.Comments?.Count ?? 0} comments");
            Console.PrintDebug($"Task involves {task.DeveloperWorkTimes.Count} developer(s)");
            
            if (task.DeveloperStartTimes.Any())
            {
                var earliestStart = task.DeveloperStartTimes.Values.Min();
                var latestStart = task.DeveloperStartTimes.Values.Max();
                Console.PrintDebug($"Earliest start: {earliestStart:yyyy-MM-dd HH:mm:ss}");
                
                if (earliestStart != latestStart)
                {
                    Console.PrintDebug($"Latest start: {latestStart:yyyy-MM-dd HH:mm:ss}");
                }
            }
        }
    }
}
