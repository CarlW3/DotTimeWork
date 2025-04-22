using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class StartTaskCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        public StartTaskCommand(ITaskTimeTracker taskTimeTracker) : base("Start", "Starts a new tracking")
        {
            AddAlias("New");
            _taskTimeTracker = taskTimeTracker;
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.VerboseLogging);
            Description = "Can start / mark TimeStamp when a new Task was started working on.";
        }

        private void Execute(string taskId, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            string description = string.Empty;
            if (string.IsNullOrWhiteSpace(taskId))
            {
                bool taskExists = false;
                do
                {
                    taskId = AnsiConsole.Ask<string>("Task ID to create:");
                    taskExists = TaskExists(taskId);
                    if (taskExists)
                    {
                        Console.WriteLine($"Task '{taskId}' already exists. Please enter a different task ID.");
                    }
                } while (taskExists);
                description = AnsiConsole.Ask<string>("Small Task Description:");
            }

            else
            {
                description = "-defined by system-";
                Console.WriteLine($"Starting task '{taskId}'.");
            }
            if (verboseLogging)
            {
                AnsiConsole.MarkupLine($"[grey]Task creation started....[/]");
            }
            _taskTimeTracker.StartTask(new TaskCreationData { Name = taskId, Description = description });
        }

        private bool TaskExists(string taskId)
        {
            return _taskTimeTracker.GetTaskById(taskId) != null;
        }
    }
}
