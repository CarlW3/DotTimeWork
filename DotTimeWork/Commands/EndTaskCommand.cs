using DotTimeWork.ConsoleService;
using DotTimeWork.TimeTracker;
using Spectre.Console;
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
        }

        internal void Execute(string taskId, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                taskId = GetTaskToWorkOn();
            }
            else
            {
                Console.WriteLine($"Ending task '{taskId}'.");
            }
            var duration = _taskTimeTracker.EndTask(taskId);
            Console.WriteLine($"Task '{taskId}' ended with durarion '{duration}'");
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
                     .Title("Select [green]Task[/] to finish?")
                     .PageSize(5)
                     .AddChoices(allTasks.Select(x => x.Name)));

        }
    }
}
