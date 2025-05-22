using DotTimeWork.ConsoleService;
using DotTimeWork.Developer;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class CommentCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IDeveloperConfigController _developerConfigController;
        private readonly IInputAndOutputService _inputAndOutputService;
        public CommentCommand(ITaskTimeTracker taskTimeTracker, IDeveloperConfigController developerConfigController,  IInputAndOutputService inputAndOutputService) : base("Comment", "Add a comment to the current task")
        {
            _taskTimeTracker = taskTimeTracker;
            _inputAndOutputService = inputAndOutputService;
            _developerConfigController = developerConfigController;
            AddOption(PublicOptions.TaskIdOption);
            AddOption(PublicOptions.CommentText);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.CommentText, PublicOptions.VerboseLogging);
            Description = "Add a comment to the current task. This will add a comment to the current task. The comment will be used to store the task details and notes.";
        }

        internal void Execute(string taskId, string commentText, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            string nameOfDeveloper = _developerConfigController.CurrentDeveloperConfig?.Name ?? "N/A";

            if (string.IsNullOrEmpty(taskId))
            {
                var availableTasks = _taskTimeTracker.GetAllRunningTasks().Select(x => x.Name).ToArray();
                taskId = _inputAndOutputService.ShowTaskSelection(availableTasks, "Select [green]Task[/] to comment?");
            }
            else if (verboseLogging)
            {
                _inputAndOutputService.PrintDebug($"Commenting task '{taskId}'.");
            }

            var selectedTask = _taskTimeTracker.GetTaskById(taskId);
            if (string.IsNullOrEmpty(commentText))
            {
                var comment = AnsiConsole.Ask<string>("Please enter the comment:");
                if (string.IsNullOrEmpty(comment))
                {
                    AnsiConsole.MarkupLine($"[red]Comment cannot be empty.[/]");
                    return;
                }
                commentText = comment;
            }

            if (selectedTask != null)
            {
                selectedTask.AddComment(new TaskComment
                {
                    Created = DateTime.Now,
                    Developer = nameOfDeveloper,
                    Comment = commentText
                });
                _taskTimeTracker.UpdateTask(selectedTask);
                AnsiConsole.MarkupLine($"[green]Comment added to task '{taskId}'.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Task '{taskId}' not found.[/]");
            }
        }


    }
}
