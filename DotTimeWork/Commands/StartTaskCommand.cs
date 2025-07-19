using DotTimeWork.Commands.Base;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class StartTaskCommand : BaseCommand
    {
        private readonly ITaskTimeTracker _taskTimeTracker;

        public StartTaskCommand(ITaskTimeTracker taskTimeTracker) : base("Start", Properties.Resources.StartTask_Description)
        {
            _taskTimeTracker = taskTimeTracker ?? throw new ArgumentNullException(nameof(taskTimeTracker));
            AddAlias("New");
        }

        protected override void SetupCommand()
        {
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.VerboseLogging);
        }

        public void Execute(string? taskId, bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                var taskCreationData = GetTaskCreationData(taskId);
                
                if (verboseLogging)
                {
                    Console.PrintInfo("Task creation started...");
                }

                _taskTimeTracker.StartTask(taskCreationData);
                Console.PrintSuccess($"Task '{taskCreationData.Name}' started successfully!");
            }, verboseLogging);
        }

        private TaskCreationData GetTaskCreationData(string? taskId)
        {
            if (!string.IsNullOrWhiteSpace(taskId))
            {
                return new TaskCreationData
                {
                    Name = taskId,
                    Description = "-defined by system-",
                    TaskType = TaskType.Other
                };
            }

            // Interactive mode - prompt for task details
            var name = Console.AskForInput<string>(Properties.Resources.StartTask_CreateTask);
            var description = Console.AskForInput<string>(Properties.Resources.StartTask_CreateTask_SmallDescription);
            var taskType = PromptForTaskType();

            return new TaskCreationData
            {
                Name = name,
                Description = description,
                TaskType = taskType
            };
        }

        private TaskType PromptForTaskType()
        {
            var typeChoices = Enum.GetNames(typeof(TaskType));
            var selectedType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select task type:")
                    .AddChoices(typeChoices));

            return Enum.TryParse<TaskType>(selectedType, out var parsedType) 
                ? parsedType 
                : TaskType.Other;
        }
    }
}
