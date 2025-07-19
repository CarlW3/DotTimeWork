using DotTimeWork.TimeTracker;

namespace DotTimeWork.Services
{
    /// <summary>
    /// ToDo: Add a caching way -> One calls trigger loading all Tasks and then other method uses this cache until finished
    /// </summary>
    internal class TotalWorkingTimeCalculator : ITotalWorkingTimeCalculator
    {
        private readonly ITaskTimeTracker _taskTimeTracker;
        private List<TaskData> _runningTasks;
        private List<TaskData> _finishedTasks;

        public TotalWorkingTimeCalculator(ITaskTimeTracker taskTimeTracker)
        {
            _taskTimeTracker = taskTimeTracker;
        }

        public void LoadTasks()
        {
            _runningTasks = _taskTimeTracker.GetAllRunningTasks().ToList();
            _finishedTasks = _taskTimeTracker.GetAllFinishedTasks().ToList();
        }

        public int TotalMinutesFocusWorkingTime
        {
            get
            {
                var combinedTasks = _runningTasks.Concat(_finishedTasks);

                int totalMinutes = 0;
                foreach (var task in combinedTasks.SelectMany(t => t.DeveloperWorkTimes))
                {
                    totalMinutes += task.Value;
                }
                return totalMinutes;
            }
        }

        public TimeSpan GetTotalTimeFinishedTasks()
        {
            if (_finishedTasks == null || !_finishedTasks.Any())
            {
                return TimeSpan.Zero;
            }

            TimeSpan totalTime = TimeSpan.Zero;
            foreach (var task in _finishedTasks)
            {
                // Calculate for each developer separately
                foreach (var startTime in task.DeveloperStartTimes)
                {
                    totalTime += (task.Finished - startTime.Value);
                }
            }
            return totalTime;
        }

        public TimeSpan GetTotalTimeRunningTasks()
        {
            if (_runningTasks == null || !_runningTasks.Any())
            {
                return TimeSpan.Zero;
            }

            TimeSpan totalTime = TimeSpan.Zero;
            DateTime now = DateTime.Now;
            foreach (var task in _runningTasks)
            {
                // Calculate for each developer separately
                foreach (var startTime in task.DeveloperStartTimes)
                {
                    totalTime += (now - startTime.Value);
                }
            }
            return totalTime;
        }

        public TimeSpan GetWorkingSpanTime()
        {
            var combined = _runningTasks.Concat(_finishedTasks);
            // Get the earliest start time across all developers
            var earliestStart = combined.SelectMany(t => t.DeveloperStartTimes.Values).Min();
            var latestEnd = combined.Max(task => task.Finished);
            if (latestEnd == DateTime.MinValue)
            {
                latestEnd = DateTime.Now; // If no task is finished, use current time
            }
            return latestEnd - earliestStart;

        }

    }
}
