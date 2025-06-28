using DotTimeWork.ConsoleService;
using DotTimeWork.TimeTracker;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class EndTaskCommand: Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IInputAndOutputService _inputAndOutputService;
        public EndTaskCommand(ITaskTimeTracker taskTimeTracker,IInputAndOutputService inputAndOutputService) : base("End", "Ends the current tracking")
        {
            _taskTimeTracker = taskTimeTracker;
            _inputAndOutputService = inputAndOutputService;
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute,PublicOptions.TaskIdOption,PublicOptions.VerboseLogging);
            Description = "Marks task as finished";

            Command allTasks=new Command("All", "Ends all running tasks");
            allTasks.SetHandler(ExecuteAllTasks, PublicOptions.VerboseLogging);
            AddCommand(allTasks);
        }

        private void ExecuteAllTasks(bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            // Get all running tasks
            var runningTasks = _taskTimeTracker.GetAllRunningTasks()
                // Filter to only include tasks assigned to current developer
                .Where(task => _taskTimeTracker.IsTaskAssignedToCurrentDeveloper(task.Name))
                .ToList();
                
            if (runningTasks.Count == 0)
            {
                _inputAndOutputService.PrintNormal("No running tasks found for your account.");
                return;
            }
            if (verboseLogging)
            {
                _inputAndOutputService.PrintDebug($"Stopping {runningTasks.Count} tasks now...");
            }
            int counter = 0;
            foreach (var task in runningTasks)
            {
                var duration = _taskTimeTracker.EndTask(task.Name);
                _inputAndOutputService.PrintNormal($"Task '{task.Name}' ended with duration '{duration}'");
                counter++;
            }
            if (verboseLogging)
            {
                _inputAndOutputService.PrintDebug($"All {counter} tasks ended");
            }
        }

        internal void Execute(string taskId, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                // Only show tasks for the current developer
                var currentDeveloperTasks = _taskTimeTracker.GetAllRunningTasks()
                    .Where(x => _taskTimeTracker.IsTaskAssignedToCurrentDeveloper(x.Name))
                    .Select(x => x.Name)
                    .ToArray();
                
                if (currentDeveloperTasks.Length == 0)
                {
                    _inputAndOutputService.PrintNormal("You have no running tasks.");
                    return;
                }
                
                taskId = _inputAndOutputService.ShowTaskSelection(currentDeveloperTasks, "Select [green]Task[/] to finish?");
            }
            else
            {
                Console.WriteLine($"Ending task '{taskId}'.");
            }
            var duration = _taskTimeTracker.EndTask(taskId);
            _inputAndOutputService.PrintNormal($"Task '{taskId}' ended with durarion '{duration}'");
        }
    }
}
