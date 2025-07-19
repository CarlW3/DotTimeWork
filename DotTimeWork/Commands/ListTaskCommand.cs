using DotTimeWork.Commands.Base;
using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class ListTaskCommand : BaseCommand
    {
        private readonly ITaskTimeTracker _taskTimeTracker;

        public ListTaskCommand(ITaskTimeTracker taskTimeTracker) : base("List", Properties.Resources.List_Description)
        {
            _taskTimeTracker = taskTimeTracker ?? throw new ArgumentNullException(nameof(taskTimeTracker));
        }

        protected override void SetupCommand()
        {
            this.SetHandler(Execute, PublicOptions.VerboseLogging);
        }

        public void Execute(bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                var tasks = _taskTimeTracker.GetAllRunningTasks();
                
                if (!tasks.Any())
                {
                    Console.PrintInfo(Properties.Resources.General_NoTaskAvailable);
                    return;
                }

                if (verboseLogging)
                {
                    Console.PrintDebug($"Found {tasks.Count} running tasks");
                }

                DisplayTasksTable(tasks);
            }, verboseLogging);
        }

        private void DisplayTasksTable(IList<TaskData> tasks)
        {
            var table = CreateTasksTable();
            var now = DateTime.Now;

            foreach (var task in tasks)
            {
                AddTaskRowsToTable(table, task, now);
            }

            AnsiConsole.Write(table);
        }

        private static Table CreateTasksTable()
        {
            var table = new Table();
            table.AddColumn(Properties.Resources.List_Column_TaskName);
            table.AddColumn(Properties.Resources.List_Column_Developer);
            table.AddColumn(Properties.Resources.List_Column_StartTime);
            table.AddColumn(Properties.Resources.List_Column_WorkingTime);
            table.AddColumn(Properties.Resources.List_Column_FocusTime);
            return table;
        }

        private void AddTaskRowsToTable(Table table, TaskData task, DateTime now)
        {
            foreach (var (developerName, focusTime) in task.DeveloperWorkTimes)
            {
                var startTime = GetDeveloperStartTime(task, developerName);
                var workingTime = CalculateWorkingTime(now, startTime);
                
                table.AddRow(
                    task.Name,
                    developerName,
                    startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeHelper.GetWorkingTimeHumanReadable(workingTime),
                    TimeHelper.GetWorkingTimeHumanReadable(focusTime)
                );
            }
        }

        private DateTime GetDeveloperStartTime(TaskData task, string developerName)
        {
            var startTime = task.GetDeveloperStartTime(developerName);
            
            // Fix missing start time data
            if (startTime == DateTime.MinValue)
            {
                startTime = task.Created;
                task.SetDeveloperStartTime(developerName, startTime);
                _taskTimeTracker.UpdateTask(task);
                
                if (PublicOptions.IsVerbosLogging)
                {
                    Console.PrintDebug($"Fixed missing start time for developer '{developerName}' on task '{task.Name}'");
                }
            }
            
            return startTime;
        }

        private static int CalculateWorkingTime(DateTime now, DateTime startTime)
        {
            return (int)(now - startTime).TotalMinutes;
        }
    }
}
