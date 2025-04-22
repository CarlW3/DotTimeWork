using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class EndTaskCommand: Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        public EndTaskCommand(ITaskTimeTracker taskTimeTracker) : base("End", "Ends the current tracking")
        {
            _taskTimeTracker = taskTimeTracker;
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute,PublicOptions.TaskIdOption,PublicOptions.VerboseLogging);
            Description = "Marks task as finished";
        }

        private void Execute(string taskId, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                taskId = AnsiConsole.Ask<string>("Please define the Task to end:");
            }
            else
            {
                Console.WriteLine($"Ending task '{taskId}'.");
            }
            var duration = _taskTimeTracker.EndTask(taskId);
            Console.WriteLine($"Task '{taskId}' ended with durarion '{duration}'");
        }
    }
}
