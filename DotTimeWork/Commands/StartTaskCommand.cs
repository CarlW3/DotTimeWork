using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class StartTaskCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        public StartTaskCommand(ITaskTimeTracker taskTimeTracker) : base("Start", Properties.Resources.StartTask_Description)
        {
            AddAlias("New");
            _taskTimeTracker = taskTimeTracker;
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.VerboseLogging);
            Description = Properties.Resources.StartTask_Description;
        }

        private void Execute(string taskId, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            string description = string.Empty;
            TaskType taskType = TaskType.Other;
            if (string.IsNullOrWhiteSpace(taskId))
            {
                bool taskExists = false;
                do
                {
                    taskId = AnsiConsole.Ask<string>(Properties.Resources.StartTask_CreateTask);
                    taskExists = TaskExists(taskId);
                    if (taskExists)
                    {
                        Console.WriteLine(string.Format(Properties.Resources.StartTask_CreateTask_Failed, taskId));
                    }
                } while (taskExists);
                description = AnsiConsole.Ask<string>(Properties.Resources.StartTask_CreateTask_SmallDescription);

                // Prompt for TaskType (optional)
                var typeChoices = Enum.GetNames(typeof(TaskType));
                var selectedType = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select task type:")
                        .AddChoices(typeChoices));
                if (Enum.TryParse<TaskType>(selectedType, out var parsedType))
                {
                    taskType = parsedType;
                }
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
            _taskTimeTracker.StartTask(new TaskCreationData
            {
                Name = taskId,
                Description = description,
                TaskType = taskType
            });
        }

        private bool TaskExists(string taskId)
        {
            return _taskTimeTracker.GetTaskById(taskId) != null;
        }
    }
}
