using DotTimeWork.TimeTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTimeWork.Services
{
    /// <summary>
    /// ToDo: Add a caching way -> One calls trigger loading all Tasks and then other method uses this cache until finished
    /// </summary>
    internal class TotalWorkingTimeCalculator : ITotalWorkingTimeCalculator
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        public TotalWorkingTimeCalculator(ITaskTimeTracker taskTimeTracker)
        {
            _taskTimeTracker = taskTimeTracker;
        }

        public int TotalMinutesFocusWorkingTime
        {
            get
            {
                var allFinishedTasks = _taskTimeTracker.GetAllFinishedTasks();
                var allRunningTasks = _taskTimeTracker.GetAllRunningTasks();
                var combinedTasks = allFinishedTasks.Concat(allRunningTasks);

                int totalMinutes = 0;
                foreach (var task in combinedTasks)
                {
                    totalMinutes += task.FocusWorkTime;
                }
                return totalMinutes;
            }
        }

        public TimeSpan GetTotalTimeFinishedTasks()
        {
            var allFinishedTasks = _taskTimeTracker.GetAllFinishedTasks();
            if (allFinishedTasks == null || !allFinishedTasks.Any())
            {
                return TimeSpan.Zero;
            }

            TimeSpan totalTime = TimeSpan.Zero;
            foreach (var task in allFinishedTasks)
            {
                totalTime += (task.Finished - task.Started);
            }
            return totalTime;
        }

        public TimeSpan GetTotalTimeRunningTasks()
        {
            var allStartedTasks = _taskTimeTracker.GetAllRunningTasks();
            if (allStartedTasks == null || !allStartedTasks.Any())
            {
                return TimeSpan.Zero;
            }

            TimeSpan totalTime = TimeSpan.Zero;
            foreach (var task in allStartedTasks)
            {
                totalTime += (DateTime.Now - task.Started);
            }
            return totalTime;
        }

        public TimeSpan GetWorkingSpanTime()
        {
            var allFinishedTasks = _taskTimeTracker.GetAllFinishedTasks();
            var allStartedTasks = _taskTimeTracker.GetAllRunningTasks();
            var combined = allFinishedTasks.Concat(allStartedTasks);
            var earliestStart = combined.Min(task => task.Started);
            var latestEnd = combined.Max(task => task.Finished);
            if (latestEnd == DateTime.MinValue)
            {
                latestEnd = DateTime.Now; // If no task is finished, use current time
            }
            return latestEnd - earliestStart;

        }

    }
}
