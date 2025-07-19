using DotTimeWork.Commands.Base;
using DotTimeWork.Developer;
using DotTimeWork.TimeTracker;
using DotTimeWork.Validation;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class CommentCommand : BaseCommand
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IDeveloperConfigController _developerConfigController;

        public CommentCommand(
            ITaskTimeTracker taskTimeTracker, 
            IDeveloperConfigController developerConfigController) 
            : base("Comment", "Add a comment to a task")
        {
            _taskTimeTracker = taskTimeTracker ?? throw new ArgumentNullException(nameof(taskTimeTracker));
            _developerConfigController = developerConfigController ?? throw new ArgumentNullException(nameof(developerConfigController));
        }

        protected override void SetupCommand()
        {
            AddOption(PublicOptions.TaskIdOption);
            AddOption(PublicOptions.CommentText);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.CommentText, PublicOptions.VerboseLogging);
            Description = "Add a comment to a task. Comments help track task progress and store important notes.";
        }

        private void Execute(string? taskId, string? commentText, bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                var selectedTaskId = GetTaskIdForComment(taskId, verboseLogging);
                if (string.IsNullOrEmpty(selectedTaskId))
                {
                    Console.PrintWarning("No task selected for commenting.");
                    return;
                }

                var task = GetTaskForComment(selectedTaskId);
                if (task == null)
                {
                    Console.PrintError($"Task '{selectedTaskId}' not found.");
                    return;
                }

                var finalCommentText = GetCommentText(commentText);
                if (string.IsNullOrEmpty(finalCommentText))
                {
                    Console.PrintWarning("Comment cannot be empty.");
                    return;
                }

                AddCommentToTask(task, finalCommentText, selectedTaskId);
            }, verboseLogging);
        }

        private string? GetTaskIdForComment(string? providedTaskId, bool verboseLogging)
        {
            if (!string.IsNullOrWhiteSpace(providedTaskId))
            {
                if (verboseLogging)
                {
                    Console.PrintDebug($"Using provided task ID: '{providedTaskId}'");
                }
                return providedTaskId;
            }

            var availableTasks = _taskTimeTracker.GetAllRunningTasks()
                .Select(x => x.Name)
                .ToArray();

            if (!availableTasks.Any())
            {
                Console.PrintWarning("No running tasks available for commenting.");
                return null;
            }

            return Console.ShowTaskSelection(availableTasks, "Select [green]Task[/] to comment?");
        }

        private TaskData? GetTaskForComment(string taskId)
        {
            try
            {
                return _taskTimeTracker.GetTaskById(taskId);
            }
            catch (Exception ex)
            {
                Console.PrintError($"Error retrieving task '{taskId}': {ex.Message}");
                return null;
            }
        }

        private string? GetCommentText(string? providedCommentText)
        {
            if (!string.IsNullOrWhiteSpace(providedCommentText))
            {
                return ValidateAndReturnComment(providedCommentText);
            }

            var userComment = Console.AskForInput<string>("Please enter the comment:");
            return ValidateAndReturnComment(userComment);
        }

        private static string? ValidateAndReturnComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return null;
            }

            // Basic validation - could be expanded with ValidationHelpers
            if (comment.Length > 1000)
            {
                return comment[..1000]; // Truncate if too long
            }

            return comment.Trim();
        }

        private void AddCommentToTask(TaskData task, string commentText, string taskId)
        {
            try
            {
                var developerName = GetCurrentDeveloperName();
                
                var taskComment = new TaskComment
                {
                    Created = DateTime.Now,
                    Developer = developerName,
                    Comment = commentText
                };

                task.AddComment(taskComment);
                _taskTimeTracker.UpdateTask(task);
                
                Console.PrintSuccess($"Comment added to task '{taskId}' successfully!");
                
                if (PublicOptions.IsVerbosLogging)
                {
                    Console.PrintDebug($"Comment length: {commentText.Length} characters");
                }
            }
            catch (Exception ex)
            {
                Console.PrintError($"Failed to add comment to task '{taskId}': {ex.Message}");
            }
        }

        private string GetCurrentDeveloperName()
        {
            try
            {
                return _developerConfigController.CurrentDeveloperConfig?.Name ?? "Unknown Developer";
            }
            catch (Exception ex)
            {
                if (PublicOptions.IsVerbosLogging)
                {
                    Console.PrintWarning($"Could not load developer config: {ex.Message}");
                }
                return "Unknown Developer";
            }
        }
    }
}
