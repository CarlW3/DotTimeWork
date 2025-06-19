using DotTimeWork.ConsoleService;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class WorkCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IInputAndOutputService _inputAndOutputService;
        public WorkCommand(ITaskTimeTracker taskTimeTracker,IInputAndOutputService inputAndOutputService) : base("Work", "Active Time Tracking for a specific Task - Focus Work")
        {
            _taskTimeTracker = taskTimeTracker;
            _inputAndOutputService = inputAndOutputService;
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.VerboseLogging);
        }

        private void Execute(string taskId, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                var availableTasks = _taskTimeTracker.GetAllRunningTasks().Select(x => x.Name).ToArray();
                taskId = _inputAndOutputService.ShowTaskSelection(availableTasks, "Select [green]Task[/] for working?");
            }
            if (verboseLogging)
            {
                _inputAndOutputService.PrintDebug($"Working on task '{taskId}'.");
            }
            var selectedTask = _taskTimeTracker.GetTaskById(taskId);
            if (selectedTask != null)
            {
                // Prompt for developer name, default to current
                string currentDev = System.Environment.UserName;
                if (selectedTask.DeveloperWorkTimes.Count > 0)
                {
                    currentDev = selectedTask.DeveloperWorkTimes.ContainsKey(currentDev) ? currentDev : selectedTask.DeveloperWorkTimes.Keys.First();
                }
                string developer = AnsiConsole.Ask<string>("Enter developer name for booking time:", currentDev);

                int howManyMinutes = AnsiConsole.Ask<int>("How many minutes do you want to work on this task? [green](default: 10)[/]", 10);
                bool addBreaks = AnsiConsole.Confirm("Do you want to add breaks? [green](default: no)[/]", false);
                int breakTimeInSeconds = 0;
                if (addBreaks)
                {
                    breakTimeInSeconds = AnsiConsole.Ask<int>("How many minutes do you want to add for breaks? [green](default: 5)[/]", 5) * 60;
                }
                int totalWorkingTimeInSeconds = howManyMinutes * 60;
                int oneShift = addBreaks ? totalWorkingTimeInSeconds / 2 : totalWorkingTimeInSeconds;

                WorkOnTask(selectedTask, oneShift);
                if (addBreaks)
                {
                    Console.Beep();
                    HaveABreak(breakTimeInSeconds);
                    AnsiConsole.MarkupLine($"[red bold]Break time is over![/]");
                    Console.Beep();
                    Thread.Sleep(200);
                    Console.Beep();
                    WorkOnTask(selectedTask, oneShift);
                }
                Console.Beep();
                Thread.Sleep(200);
                Console.Beep();
                _taskTimeTracker.AddFocusTimeWork(taskId, howManyMinutes, developer);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Task with ID '{taskId}' not found.[/]");

            }
        }

        private static void HaveABreak(int breakTimeInSeconds)
        {
            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask($"[yellow]Have a short Break[/]", new ProgressTaskSettings { MaxValue = breakTimeInSeconds });

                    while (!ctx.IsFinished)
                    {
                        task1.Increment(1);
                        Thread.Sleep(1000);
                    }
                });
        }

        private static void WorkOnTask(TaskData selectedTask, int firstShift)
        {
            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask($"[green]Working on {selectedTask.Name}[/]", new ProgressTaskSettings { MaxValue = firstShift });

                    while (!ctx.IsFinished)
                    {
                        task1.Increment(1);
                        Thread.Sleep(1000);
                    }
                });
        }
    }
}