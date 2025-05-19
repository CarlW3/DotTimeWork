using DotTimeWork.ConsoleService;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class CommentCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IInputAndOutputService _inputAndOutputService;
        public CommentCommand(ITaskTimeTracker taskTimeTracker, IInputAndOutputService inputAndOutputService) : base("Comment", "Add a comment to the current task")
        {
            _taskTimeTracker = taskTimeTracker;
            _inputAndOutputService = inputAndOutputService;
            AddOption(PublicOptions.TaskIdOption);
            AddOption(PublicOptions.CommentText);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.CommentText, PublicOptions.VerboseLogging);
            Description = "Add a comment to the current task. This will add a comment to the current task. The comment will be used to store the task details and notes.";
        }

        internal void Execute(string taskId, string commentText, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                taskId = GetTaskToWorkOn();
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
                selectedTask.AddComment(commentText);
                _taskTimeTracker.UpdateTask(selectedTask);
                AnsiConsole.MarkupLine($"[green]Comment added to task '{taskId}'.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Task '{taskId}' not found.[/]");
            }
        }

        private string GetTaskToWorkOn()
        {
            var allTasks = _taskTimeTracker.GetAllRunningTasks();
            if (allTasks == null || allTasks.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]No tasks found. Please create a task first.[/]");
                return string.Empty;
            }
            return AnsiConsole.Prompt(
                 new SelectionPrompt<string>()
                     .Title("Select [green]Task[/] to add a Comment?")
                     .PageSize(5)
                     .AddChoices(allTasks.Select(x => x.Name)));

        }
    }
}
