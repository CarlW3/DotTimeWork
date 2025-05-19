using DotTimeWork.ConsoleService;
using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using System.CommandLine;

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
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption,PublicOptions.VerboseLogging);
        }

        private void Execute(string taskId,bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                taskId = _inputAndOutputService.AskForInput<string>("Please define the Task to show details for:");
            }
            else
            {
                _inputAndOutputService.PrintInfo($"Showing details for task '{taskId}'.");
            }
            var selectedTask = _taskTimeTracker.GetTaskById(taskId);
            if (selectedTask != null)
            {
                _inputAndOutputService.PrintMarkup($"[green]Task Name:[/] {selectedTask.Name}");
                _inputAndOutputService.PrintMarkup($"[green]Developer:[/] {selectedTask.Developer}");
                if(!string.IsNullOrWhiteSpace(selectedTask.Description))
                {
                    _inputAndOutputService.PrintMarkup($"[green]Description:[/] {selectedTask.Description}");
                }
                _inputAndOutputService.PrintMarkup($"[green]Start Time:[/] {selectedTask.Started.ToString()}");
                _inputAndOutputService.PrintMarkup($"[green]Working Time (Minutes):[/] {TimeHelper.GetWorkingTimeHumanReadable((int)((DateTime.Now - selectedTask.Started).TotalMinutes))}");
                _inputAndOutputService.PrintMarkup($"[green]Focus Working Time (Minutes):[/] {TimeHelper.GetWorkingTimeHumanReadable(selectedTask.FocusWorkTime)}");
            }
        }
    }
}
