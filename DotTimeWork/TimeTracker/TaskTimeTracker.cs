using DotTimeWork.DataProvider;
using DotTimeWork.Developer;
using DotTimeWork.Helper;
using DotTimeWork.Project;

namespace DotTimeWork.TimeTracker
{
    internal class TaskTimeTracker : ITaskTimeTracker
    {
        private readonly IProjectConfigController _projectConfigController;
        private readonly IDeveloperConfigController _developerConfigController;
        private readonly ITaskTimeTrackerDataProvider _taskTimeTrackerDataProvider;

        public TaskTimeTracker(IProjectConfigController projectConfigController, IDeveloperConfigController developerConfigController, ITaskTimeTrackerDataProvider taskTimeTrackerDataProvider)
        {
            _projectConfigController = projectConfigController;
            _developerConfigController = developerConfigController;
            _taskTimeTrackerDataProvider = taskTimeTrackerDataProvider;
        }

        public void StartTask(TaskCreationData creationData)
        {
            UpdateTimeTrackingFolder();
            var currentDeveloper = _developerConfigController.CurrentDeveloperConfig;
            TaskData? foundTask = _taskTimeTrackerDataProvider.GetRunningTaskById(creationData.Name);
            if (foundTask != null)
            {
                Console.WriteLine($"Task {creationData.Name} has already been started.");
                return;
            }
            var newTask = new TaskData
            {
                Name = creationData.Name,
                Started = DateTime.Now,
                Description = creationData.Description,
                Developer = currentDeveloper.Name
            };
            _developerConfigController.AssignTaskToCurrentDeveloper(newTask.Name);
            _taskTimeTrackerDataProvider.AddTask(newTask);
            Console.WriteLine($"Task {newTask.Name} started at {newTask.Started}.");
        }



        public TimeSpan EndTask(string taskId)
        {
            Guard.AgainstNullOrEmpty(taskId, nameof(taskId));

            UpdateTimeTrackingFolder();

            TaskData? taskToFinish= _taskTimeTrackerDataProvider.GetRunningTaskById(taskId);

            if (taskToFinish==null)
            {
                Console.WriteLine($"Task {taskId} has not been started.");
                return TimeSpan.Zero;
            }
            
            _taskTimeTrackerDataProvider.SetTaskAsFinished(taskToFinish);


            TimeSpan duration = taskToFinish.Finished - taskToFinish.Started;
            Console.WriteLine($"Task {taskToFinish.Name} ended at {taskToFinish.Finished}. Duration: {duration}.");

            return duration;
        }

        public TaskData GetTaskById(string taskId)
        {
            UpdateTimeTrackingFolder();
            TaskData? foundRunning = _taskTimeTrackerDataProvider.GetRunningTaskById(taskId);
            if(foundRunning != null)
            {
                return foundRunning;
            }
            TaskData? foundFinished = _taskTimeTrackerDataProvider.GetFinishedTaskById(taskId);
            if (foundFinished != null)
            {
                return foundFinished;
            }
            Console.WriteLine($"Task {taskId} not found.");
            return null;
        }

        public void AddFocusTimeWork(string taskId,int finishedMinutes)
        {
            UpdateTimeTrackingFolder();
            _taskTimeTrackerDataProvider.AddFocusTimeForTask(taskId, finishedMinutes);
        }
       

        public List<TaskData> GetAllRunningTasks()
        {
            UpdateTimeTrackingFolder();
            return _taskTimeTrackerDataProvider.RunningTasks.Values.ToList();
        }

        public List<TaskData> GetAllFinishedTasks()
        {
            UpdateTimeTrackingFolder();
            return _taskTimeTrackerDataProvider.FinishedTasks.Values.ToList();
        }

        private void UpdateTimeTrackingFolder()
        {
            ProjectConfig foundProject= _projectConfigController.GetCurrentProjectConfig();
            if(foundProject == null)
            {
                Console.WriteLine("No project found.");
                return;
            }
            string timeTrackingFolder = foundProject.TimeTrackingFolder;
            if (string.IsNullOrEmpty(timeTrackingFolder))
            {
                Console.WriteLine("No time tracking folder found.");
                return;
            }
            _taskTimeTrackerDataProvider.SetStoragePath(timeTrackingFolder);
        }

        public void UpdateTask(TaskData selectedTask)
        {
            _taskTimeTrackerDataProvider.UpdateTask(selectedTask);
        }
    }
}
