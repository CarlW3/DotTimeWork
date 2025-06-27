using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class ListTaskCommand : Command
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        public ListTaskCommand(ITaskTimeTracker taskTimeTracker) : base("List", "Lists all tasks")
        {
            _taskTimeTracker = taskTimeTracker;
            this.SetHandler(Execute);
            Description = Properties.Resources.List_Description;
        }

        private void Execute()
        {
            var tasks = _taskTimeTracker.GetAllRunningTasks();
            if (tasks.Count == 0)
            {
                Console.WriteLine(Properties.Resources.General_NoTaskAvailable);
            }
            else
            {
                Table table = new Table();
                table.AddColumn(Properties.Resources.List_Column_TaskName);
                table.AddColumn(Properties.Resources.List_Column_Developer);
                table.AddColumn(Properties.Resources.List_Column_StartTime);
                table.AddColumn(Properties.Resources.List_Column_WorkingTime);
                table.AddColumn(Properties.Resources.List_Column_FocusTime);
                DateTime now = DateTime.Now;
                foreach (var task in tasks)
                {
                    foreach (var kvp in task.DeveloperWorkTimes)
                    {
                        // Get the start time for this specific developer
                        DateTime developerStartTime = task.GetDeveloperStartTime(kvp.Key);
                        table.AddRow(
                            task.Name,
                            kvp.Key,
                            developerStartTime.ToString(),
                            TimeHelper.GetWorkingTimeHumanReadable((int)((now - developerStartTime).TotalMinutes)),
                            TimeHelper.GetWorkingTimeHumanReadable(kvp.Value)
                        );
                    }
                }
                AnsiConsole.Write(table);
            }
        }
    }
}
