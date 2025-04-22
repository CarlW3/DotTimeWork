using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class DetailsTaskCommand:Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;

        public DetailsTaskCommand(ITaskTimeTracker taskTimeTracker) : base("Details", "Shows details of a specific Task")
        {
            _taskTimeTracker = taskTimeTracker;
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption,PublicOptions.VerboseLogging);
        }

        private void Execute(string taskId,bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                taskId = AnsiConsole.Ask<string>("Please define the Task to show details for:");
            }
            else
            {
                Console.WriteLine($"Showing details for task '{taskId}'.");
            }
            var selectedTask = _taskTimeTracker.GetTaskById(taskId);
            if (selectedTask != null)
            {
                AnsiConsole.MarkupLine($"[green]Task Name:[/] {selectedTask.Name}");
                AnsiConsole.MarkupLine($"[green]Developer:[/] {selectedTask.Developer}");
                if(!string.IsNullOrWhiteSpace(selectedTask.Description))
                {
                    AnsiConsole.MarkupLine($"[green]Description:[/] {selectedTask.Description}");
                }
                AnsiConsole.MarkupLine($"[green]Start Time:[/] {selectedTask.Started.ToString()}");
                AnsiConsole.MarkupLine($"[green]Working Time (Minutes):[/] {TimeHelper.GetWorkingTimeHumanReadable((int)((DateTime.Now - selectedTask.Started).TotalMinutes))}");
                AnsiConsole.MarkupLine($"[green]Focus Working Time (Minutes):[/] {TimeHelper.GetWorkingTimeHumanReadable(selectedTask.FocusWorkTime)}");
            }
        }
    }
}
