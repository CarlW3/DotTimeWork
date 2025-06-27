using DotTimeWork.ConsoleService;
using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;
using System.Text;

namespace DotTimeWork.Commands
{
    internal class DetailsTaskCommand:Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IInputAndOutputService _inputAndOutputService;

        public DetailsTaskCommand(ITaskTimeTracker taskTimeTracker,IInputAndOutputService inputAndOutputService) : base("Details", "Shows details of a specific Task")
        {
            _taskTimeTracker = taskTimeTracker;
            _inputAndOutputService = inputAndOutputService;
            Description = Properties.Resources.Details_Description;
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption,PublicOptions.VerboseLogging);
        }

        private void Execute(string taskId,bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                var availableTasks = _taskTimeTracker.GetAllRunningTasks().Select(x => x.Name).ToArray();
                taskId = _inputAndOutputService.ShowTaskSelection(availableTasks, "Select [green]Task[/] for Details?");
            }
            else
            {
                _inputAndOutputService.PrintInfo($"Showing details for task '{taskId}'.");
            }
            var selectedTask = _taskTimeTracker.GetTaskById(taskId);
            if (selectedTask != null)
            {
                _inputAndOutputService.PrintMarkup($"[green]{Properties.Resources.List_Column_TaskName}:[/] {selectedTask.Name}");
                // Show all developers and their times
                if (selectedTask.DeveloperWorkTimes.Count > 0)
                {
                    var devLines = selectedTask.DeveloperWorkTimes.Select(kvp => $"[yellow]{kvp.Key}[/]: {TimeHelper.GetWorkingTimeHumanReadable(kvp.Value)}");
                    _inputAndOutputService.PrintMarkup("[green]Developer(s) & Focus Time:[/]");
                    foreach (var line in devLines)
                        _inputAndOutputService.PrintMarkup(line);
                }
                if(!string.IsNullOrWhiteSpace(selectedTask.Description))
                {
                    _inputAndOutputService.PrintMarkup($"[green]Description:[/] {selectedTask.Description}");
                }
                if(selectedTask.Comments!=null&& selectedTask.Comments.Count > 0)
                {

                    StringBuilder comments = new StringBuilder();
                    foreach (var comment in selectedTask.Comments)
                    {
                        comments.AppendLine($"[white]{comment.Created.ToString()}[/] | [yellow]{comment.Developer}[/] | {comment.Comment}");
                    }
                    Panel commentPanel = new Panel(new Markup(comments.ToString().TrimEnd()));
                    commentPanel.Header("Comments");
                    commentPanel.Border = BoxBorder.Rounded;
                    commentPanel.BorderStyle = new Style(Color.Green);
                    AnsiConsole.Write(commentPanel);
                }
                _inputAndOutputService.PrintMarkup($"[green]Creation Time:[/] {selectedTask.Created.ToString()}");
                
                // Show start time for each developer
                _inputAndOutputService.PrintMarkup($"[green]Developer Start Times:[/]");
                foreach (var startTime in selectedTask.DeveloperStartTimes)
                {
                    _inputAndOutputService.PrintMarkup($"[yellow]{startTime.Key}[/]: {startTime.Value.ToString()} - Working: {TimeHelper.GetWorkingTimeHumanReadable((int)((DateTime.Now - startTime.Value).TotalMinutes))}");
                }
            }
        }
    }
}
