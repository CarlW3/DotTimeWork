using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class WorkCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        public WorkCommand(ITaskTimeTracker taskTimeTracker) : base("Work", "Active Time Tracking for a specific Task - Focus Work")
        {
            _taskTimeTracker = taskTimeTracker;
            AddOption(PublicOptions.TaskIdOption);
            this.SetHandler(Execute, PublicOptions.TaskIdOption, PublicOptions.VerboseLogging);
        }

        private void Execute(string taskId, bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (string.IsNullOrEmpty(taskId))
            {
                taskId = GetTaskToWorkOn();
                AnsiConsole.MarkupLine($"Task to work on: [bold green]{taskId}[/]");

            }
            if (verboseLogging)
            {
                AnsiConsole.MarkupLine($"[grey]Working on task '{taskId}'...[/]");
            }
            var selectedTask = _taskTimeTracker.GetTaskById(taskId);
            if (selectedTask != null)
            {
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
                _taskTimeTracker.AddFocusTimeWork(taskId, howManyMinutes);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Task with ID '{taskId}' not found.[/]");

            }
        }

        /// <summary>
        /// Copied code to End Task
        /// </summary>
        /// <returns></returns>
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
                     .Title("Select [green]Task[/] to start?")
                     .PageSize(5)
                     .AddChoices(allTasks.Select(x => x.Name)));

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