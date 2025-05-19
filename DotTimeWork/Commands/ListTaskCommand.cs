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
            Description = "Lists all tasks. This will list all tasks in the current project. The tasks will be listed in the order they were created.";
        }

        private void Execute()
        {
            var tasks = _taskTimeTracker.GetAllRunningTasks();
            if (tasks.Count == 0)
            {
                Console.WriteLine("No tasks found.");
            }
            else
            {
                Table table = new Table();
                table.AddColumn("Task Name");
                table.AddColumn("Developer");
                table.AddColumn("Start Time");
                table.AddColumn("Working Time (Minutes)");
                table.AddColumn("Focus Working Time (Minutes)");
                DateTime now = DateTime.Now;
                foreach (var task in tasks)
                {
                    table.AddRow(task.Name, task.Developer, task.Started.ToString(), TimeHelper.GetWorkingTimeHumanReadable((int)((now - task.Started).TotalMinutes)), TimeHelper.GetWorkingTimeHumanReadable(task.FocusWorkTime));
                }
                AnsiConsole.Write(table);
            }
        }
    }
}
