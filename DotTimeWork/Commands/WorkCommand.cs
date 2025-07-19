using DotTimeWork.Commands.Base;
using DotTimeWork.Developer;
using DotTimeWork.TimeTracker;
using DotTimeWork.Validation;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class WorkCommand : BaseCommand
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private readonly IDeveloperConfigController _developerConfigController;

        public WorkCommand(
            ITaskTimeTracker taskTimeTracker,
            IDeveloperConfigController developerConfigController) 
            : base("Work", "Active Time Tracking for a specific Task - Focus Work")
        {
            _taskTimeTracker = taskTimeTracker ?? throw new ArgumentNullException(nameof(taskTimeTracker));
            _developerConfigController = developerConfigController ?? throw new ArgumentNullException(nameof(developerConfigController));
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
                var selectedTaskId = GetTaskIdForWork(taskId, verboseLogging);
                if (string.IsNullOrEmpty(selectedTaskId))
                {
                    Console.PrintWarning("No task selected for work session.");
                    return;
                }

                var task = GetTaskForWork(selectedTaskId);
                if (task == null)
                {
                    Console.PrintError($"Task '{selectedTaskId}' not found.");
                    return;
                }

                var workSession = CreateWorkSession();
                if (workSession == null)
                {
                    Console.PrintWarning("Work session was cancelled.");
                    return;
                }

                ExecuteWorkSession(task, workSession, selectedTaskId);
            }, verboseLogging);
        }

        private string? GetTaskIdForWork(string? providedTaskId, bool verboseLogging)
        {
            if (!string.IsNullOrWhiteSpace(providedTaskId))
            {
                if (verboseLogging)
                {
                    Console.PrintDebug($"Using provided task ID: '{providedTaskId}'");
                }
                return providedTaskId;
            }

            var availableTasks = GetAvailableTaskNames();
            if (!availableTasks.Any())
            {
                Console.PrintWarning("No running tasks available for work session.");
                return null;
            }

            return Console.ShowTaskSelection(availableTasks, "Select [green]Task[/] for working?");
        }

        private string[] GetAvailableTaskNames()
        {
            try
            {
                return _taskTimeTracker.GetAllRunningTasks()
                    .Select(x => x.Name)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.PrintError($"Error retrieving tasks: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        private TaskData? GetTaskForWork(string taskId)
        {
            try
            {
                return _taskTimeTracker.GetGlobalRunningTaskById(taskId);
            }
            catch (Exception ex)
            {
                Console.PrintError($"Error retrieving task: {ex.Message}");
                return null;
            }
        }

        private WorkSession? CreateWorkSession()
        {
            try
            {
                var developer = GetDeveloperForSession();
                var workTime = GetWorkTimeInMinutes();
                var breakConfig = GetBreakConfiguration();

                return new WorkSession
                {
                    DeveloperName = developer,
                    WorkTimeMinutes = workTime,
                    IncludeBreaks = breakConfig.IncludeBreaks,
                    BreakTimeMinutes = breakConfig.BreakTimeMinutes
                };
            }
            catch (Exception ex)
            {
                Console.PrintError($"Error creating work session: {ex.Message}");
                return null;
            }
        }

        private string GetDeveloperForSession()
        {
            try
            {
                var currentDeveloper = _developerConfigController.CurrentDeveloperConfig?.Name ?? "Unknown";
                var developer = AnsiConsole.Ask<string>("Enter developer name for booking time:", currentDeveloper);
                
                var validation = ValidationHelpers.ValidateDeveloperName(developer);
                if (validation.IsFailure)
                {
                    Console.PrintWarning($"Developer name validation: {validation.ErrorMessage}");
                    return currentDeveloper;
                }
                
                return developer;
            }
            catch (Exception)
            {
                return "Unknown Developer";
            }
        }

        private int GetWorkTimeInMinutes()
        {
            var workTime = AnsiConsole.Ask<int>("How many minutes do you want to work on this task? [green](default: 25)[/]", 25);
            
            var validation = ValidationHelpers.ValidateWorkTimeMinutes(workTime);
            if (validation.IsFailure)
            {
                Console.PrintWarning($"Work time validation: {validation.ErrorMessage}. Using default 25 minutes.");
                return 25;
            }
            
            return workTime;
        }

        private (bool IncludeBreaks, int BreakTimeMinutes) GetBreakConfiguration()
        {
            var includeBreaks = AnsiConsole.Confirm("Do you want to add breaks? [green](default: no)[/]", false);
            
            if (!includeBreaks)
            {
                return (false, 0);
            }

            var breakTime = AnsiConsole.Ask<int>("How many minutes do you want to add for breaks? [green](default: 5)[/]", 5);
            
            var validation = ValidationHelpers.ValidateBreakTimeMinutes(breakTime);
            if (validation.IsFailure)
            {
                Console.PrintWarning($"Break time validation: {validation.ErrorMessage}. Using default 5 minutes.");
                return (true, 5);
            }
            
            return (true, breakTime);
        }

        private void ExecuteWorkSession(TaskData task, WorkSession session, string taskId)
        {
            try
            {
                Console.PrintInfo($"Starting work session for task '{task.Name}' - {session.WorkTimeMinutes} minutes");
                
                if (session.IncludeBreaks)
                {
                    ExecuteWorkSessionWithBreaks(task, session);
                }
                else
                {
                    ExecuteSingleWorkSession(task, session.WorkTimeMinutes);
                }

                RecordFocusTime(taskId, session);
                PlayCompletionSound();
                
                Console.PrintSuccess($"Work session completed! {session.WorkTimeMinutes} minutes of focus time recorded.");
            }
            catch (Exception ex)
            {
                Console.PrintError($"Error during work session: {ex.Message}");
            }
        }

        private void ExecuteWorkSessionWithBreaks(TaskData task, WorkSession session)
        {
            var halfWorkTime = session.WorkTimeMinutes / 2;
            
            // First work period
            ExecuteSingleWorkSession(task, halfWorkTime);
            
            // Break period
            PlayNotificationSound();
            ExecuteBreakPeriod(session.BreakTimeMinutes);
            
            Console.PrintInfo("Break time is over! Back to work.");
            PlayNotificationSound();
            Thread.Sleep(200);
            PlayNotificationSound();
            
            // Second work period
            ExecuteSingleWorkSession(task, halfWorkTime);
        }

        private static void ExecuteSingleWorkSession(TaskData task, int minutes)
        {
            var seconds = minutes * 60;
            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var progressTask = ctx.AddTask($"[green]Working on {task.Name}[/]", new ProgressTaskSettings 
                    { 
                        MaxValue = seconds 
                    });

                    while (!ctx.IsFinished)
                    {
                        progressTask.Increment(1);
                        Thread.Sleep(1000);
                    }
                });
        }

        private static void ExecuteBreakPeriod(int minutes)
        {
            var seconds = minutes * 60;
            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var progressTask = ctx.AddTask("[yellow]Have a short Break[/]", new ProgressTaskSettings 
                    { 
                        MaxValue = seconds 
                    });

                    while (!ctx.IsFinished)
                    {
                        progressTask.Increment(1);
                        Thread.Sleep(1000);
                    }
                });
        }

        private void RecordFocusTime(string taskId, WorkSession session)
        {
            try
            {
                _taskTimeTracker.AddFocusTimeWork(taskId, session.WorkTimeMinutes, session.DeveloperName);
                
                if (PublicOptions.IsVerbosLogging)
                {
                    Console.PrintDebug($"Recorded {session.WorkTimeMinutes} minutes of focus time for {session.DeveloperName}");
                }
            }
            catch (Exception ex)
            {
                Console.PrintError($"Failed to record focus time: {ex.Message}");
            }
        }

        private static void PlayCompletionSound()
        {
            try
            {
                System.Console.Beep();
                Thread.Sleep(200);
                System.Console.Beep();
            }
            catch
            {
                // Ignore beep errors on systems that don't support it
            }
        }

        private static void PlayNotificationSound()
        {
            try
            {
                System.Console.Beep();
            }
            catch
            {
                // Ignore beep errors on systems that don't support it
            }
        }

        private class WorkSession
        {
            public string DeveloperName { get; set; } = string.Empty;
            public int WorkTimeMinutes { get; set; }
            public bool IncludeBreaks { get; set; }
            public int BreakTimeMinutes { get; set; }
        }
    }
}