using DotTimeWork.Commands.Base;
using DotTimeWork.TimeTracker;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class EndTaskCommand : BaseCommand
    {
        private readonly ITaskTimeTracker _taskTimeTracker;

        public EndTaskCommand(ITaskTimeTracker taskTimeTracker) : base("End", "Ends the current tracking")
        {
            _taskTimeTracker = taskTimeTracker ?? throw new ArgumentNullException(nameof(taskTimeTracker));
        }

        protected override void SetupCommand()
        {
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.VerboseLogging);

            var allTasksCommand = new Command("All", "Ends all running tasks");
            allTasksCommand.SetHandler(ExecuteAllTasks, PublicOptions.VerboseLogging);
            AddCommand(allTasksCommand);
        }

        private void Execute(string? taskId, bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                var selectedTaskId = GetTaskIdToEnd(taskId);
                if (string.IsNullOrEmpty(selectedTaskId))
                {
                    Console.PrintWarning("No task selected to end.");
                    return;
                }

                var duration = _taskTimeTracker.EndTask(selectedTaskId);
                Console.PrintSuccess($"Task '{selectedTaskId}' ended with duration '{duration}'");
            }, verboseLogging);
        }

        private void ExecuteAllTasks(bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                var runningTasks = GetCurrentDeveloperRunningTasks();
                
                if (!runningTasks.Any())
                {
                    Console.PrintInfo("No running tasks found for your account.");
                    return;
                }

                if (verboseLogging)
                {
                    Console.PrintDebug($"Stopping {runningTasks.Count()} tasks now...");
                }

                var endedCount = EndAllTasks(runningTasks);

                if (verboseLogging)
                {
                    Console.PrintDebug($"All {endedCount} tasks ended successfully");
                }
            }, verboseLogging);
        }

        private string? GetTaskIdToEnd(string? providedTaskId)
        {
            if (!string.IsNullOrWhiteSpace(providedTaskId))
            {
                return providedTaskId;
            }

            var currentDeveloperTasks = GetCurrentDeveloperRunningTasks()
                .Select(x => x.Name)
                .ToArray();

            if (!currentDeveloperTasks.Any())
            {
                Console.PrintInfo("You have no running tasks.");
                return null;
            }

            return Console.ShowTaskSelection(currentDeveloperTasks, "Select [green]Task[/] to finish?");
        }

        private IEnumerable<TaskData> GetCurrentDeveloperRunningTasks()
        {
            return _taskTimeTracker.GetAllRunningTasks()
                .Where(task => _taskTimeTracker.IsTaskAssignedToCurrentDeveloper(task.Name));
        }

        private int EndAllTasks(IEnumerable<TaskData> tasks)
        {
            var endedCount = 0;
            foreach (var task in tasks)
            {
                try
                {
                    var duration = _taskTimeTracker.EndTask(task.Name);
                    Console.PrintInfo($"Task '{task.Name}' ended with duration '{duration}'");
                    endedCount++;
                }
                catch (Exception ex)
                {
                    Console.PrintError($"Failed to end task '{task.Name}': {ex.Message}");
                }
            }
            return endedCount;
        }
    }
}
