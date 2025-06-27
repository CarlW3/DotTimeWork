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
            
            // Current time for this developer's start time
            DateTime now = DateTime.Now;
            
            // Check if the current developer has already started this task
            bool developerAlreadyStartedTask = foundTask != null && 
                foundTask.IsDeveloperParticipating(currentDeveloper.Name);
                
            if (developerAlreadyStartedTask)
            {
                Console.WriteLine($"You have already started task '{creationData.Name}'.");
                return;
            }
            
            // If the task exists but is started by a different developer, add this developer to it
            if (foundTask != null)
            {
                Console.WriteLine($"Adding you to existing task '{creationData.Name}'.");
                // Set this developer's start time to now
                foundTask.SetDeveloperStartTime(currentDeveloper.Name, now);
                // Use EnsureDeveloperEntry to safely add the developer to DeveloperWorkTimes
                foundTask.EnsureDeveloperEntry(currentDeveloper.Name);
                _developerConfigController.AssignTaskToCurrentDeveloper(foundTask.Name);
                _taskTimeTrackerDataProvider.UpdateTask(foundTask);
                Console.WriteLine($"You started task '{creationData.Name}' at {now}.");
                return;
            }
            
            // Otherwise, create a new task
            var newTask = new TaskData
            {
                Name = creationData.Name,
                Created = now,
                Description = creationData.Description,
                CreatedBy = currentDeveloper.Name,
            };
            
            // Set the start time for the creator
            newTask.SetDeveloperStartTime(currentDeveloper.Name, now);
            // Safely initialize work time for the creator using EnsureDeveloperEntry
            newTask.EnsureDeveloperEntry(currentDeveloper.Name);
            _developerConfigController.AssignTaskToCurrentDeveloper(newTask.Name);
            _taskTimeTrackerDataProvider.AddTask(newTask);
            Console.WriteLine($"Task '{newTask.Name}' started at {now}.");
        }



        public TimeSpan EndTask(string taskId)
        {
            Guard.AgainstNullOrEmpty(taskId, nameof(taskId));

            UpdateTimeTrackingFolder();
            
            var currentDeveloper = _developerConfigController.CurrentDeveloperConfig;
            TaskData? taskToFinish= _taskTimeTrackerDataProvider.GetRunningTaskById(taskId);

            if (taskToFinish==null)
            {
                Console.WriteLine($"Task {taskId} has not been started.");
                return TimeSpan.Zero;
            }
            
            // Check if this developer is participating in the task
            if (!taskToFinish.IsDeveloperParticipating(currentDeveloper.Name))
            {
                Console.WriteLine($"You are not working on task '{taskId}'.");
                return TimeSpan.Zero;
            }
            
            // Get this developer's start time
            DateTime developerStartTime = taskToFinish.GetDeveloperStartTime(currentDeveloper.Name);
            
            _taskTimeTrackerDataProvider.SetTaskAsFinished(taskToFinish);

            // Calculate duration based on this developer's start time
            TimeSpan duration = taskToFinish.Finished - developerStartTime;
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

        public TaskData GetGlobalRunningTaskById(string taskId)
        {
            UpdateTimeTrackingFolder();
            TaskData? foundRunning = _taskTimeTrackerDataProvider.GetGlobalRunningTaskById(taskId);
            return foundRunning;
        }


        public void AddFocusTimeWork(string taskId, int finishedMinutes, string developer)
        {
            UpdateTimeTrackingFolder();
            _taskTimeTrackerDataProvider.AddFocusTimeForTask(taskId, finishedMinutes, developer);
        }

        public List<TaskData> GetAllRunningTasks()
        {
            UpdateTimeTrackingFolder();
            return _taskTimeTrackerDataProvider.GetAllRunningTasksForAllDevelopers();
        }

        public List<TaskData> GetAllFinishedTasks()
        {
            UpdateTimeTrackingFolder();
            return _taskTimeTrackerDataProvider.GetAllFinishedTasksForAllDevelopers();
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
