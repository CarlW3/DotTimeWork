using DotTimeWork.Commands;
using DotTimeWork.ConsoleService;
using DotTimeWork.Developer;
using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using System.Text.Json;

namespace DotTimeWork.DataProvider
{
    internal class TaskTimeTrackerDataJson : ITaskTimeTrackerDataProvider
    {
        private Dictionary<string, TaskData>? _runningTasks;
        private Dictionary<string, TaskData>? _finishedTasks;
        private string? _storagePath;

        private readonly IInputAndOutputService _inputAndOutputService;
        private readonly IDeveloperConfigController _developerConfigController;

        private const string StartTaskDataFileNamePattern = "taskStartTimeData.{0}.json";
        private const string FinishedTaskDataFileNamePattern = "taskFinishedTimeData.{0}.json";


        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };

        public TaskTimeTrackerDataJson(IInputAndOutputService inputAndOutputService, IDeveloperConfigController developerConfigController)
        {
            _inputAndOutputService = inputAndOutputService;
            _developerConfigController = developerConfigController;
            _runningTasks = null;
            _finishedTasks = null;
        }


        /// <summary>
        /// Assigns the storage path for the task time tracker data.
        /// Íf folder does not exist, it will be created when needed.
        /// </summary>
        /// <param name="path">Filesystem path</param>
        public void SetStoragePath(string path)
        {
            Guard.AgainstNullOrEmpty(path, nameof(path));
            _storagePath = path;
        }

        public Dictionary<string, TaskData> RunningTasks
        {
            get
            {
                if (_runningTasks == null)
                {
                    _runningTasks = LoadAggregatedTaskData(true);
                }
                return _runningTasks;
            }
        }

        public Dictionary<string, TaskData> FinishedTasks
        {
            get
            {
                if (_finishedTasks == null)
                {
                    _finishedTasks = LoadAggregatedTaskData(false);
                }
                return _finishedTasks;
            }
        }

        private string GetCurrentDeveloperFileName(bool running)
        {
            var dev = _developerConfigController.CurrentDeveloperConfig?.Name ?? "unknown";
            dev = dev.Replace(" ", "_").ToLowerInvariant();
            return running ? string.Format(StartTaskDataFileNamePattern, dev) : string.Format(FinishedTaskDataFileNamePattern, dev);
        }

        private string GetDateFilePath(string file)
        {
            if (string.IsNullOrEmpty(_storagePath))
                throw new InvalidOperationException("Storage path is not set.");
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            return Path.Combine(_storagePath, file);
        }

        private Dictionary<string, TaskData> LoadTaskData(string file)
        {
            try
            {
                if (!File.Exists(GetDateFilePath(file)))
                {
                    return new Dictionary<string, TaskData>();
                }
                string json = File.ReadAllText(GetDateFilePath(file));
                var dict = JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions);
                return dict ?? new Dictionary<string, TaskData>();
            }
            catch
            {
                return new Dictionary<string, TaskData>();
            }
        }

        /// <summary>
        /// Load and aggregate task data from all developers
        /// </summary>
        private Dictionary<string, TaskData> LoadAggregatedTaskData(bool running)
        {
            var aggregatedTasks = new Dictionary<string, TaskData>();
            
            if (string.IsNullOrEmpty(_storagePath) || !Directory.Exists(_storagePath))
                return aggregatedTasks;

            var pattern = running ? "taskStartTimeData.*.json" : "taskFinishedTimeData.*.json";
            
            foreach (var file in Directory.GetFiles(_storagePath, pattern))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions);
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            string normalizedTaskId = NormalizeTaskId(kvp.Key);
                            TaskData taskFromFile = kvp.Value;
                            
                            if (aggregatedTasks.ContainsKey(normalizedTaskId))
                            {
                                // Merge with existing task
                                TaskData existingTask = aggregatedTasks[normalizedTaskId];
                                MergeTaskData(existingTask, taskFromFile);
                            }
                            else
                            {
                                // Add new task
                                aggregatedTasks[normalizedTaskId] = taskFromFile;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore files that can't be read
                }
            }
            
            return aggregatedTasks;
        }

        /// <summary>
        /// Merge task data from different developers working on the same task
        /// </summary>
        private void MergeTaskData(TaskData target, TaskData source)
        {
            // Merge developer work times
            foreach (var developerTime in source.DeveloperWorkTimes)
            {
                if (target.DeveloperWorkTimes.ContainsKey(developerTime.Key))
                {
                    target.DeveloperWorkTimes[developerTime.Key] += developerTime.Value;
                }
                else
                {
                    target.DeveloperWorkTimes[developerTime.Key] = developerTime.Value;
                }
            }

            // Merge comments
            foreach (var comment in source.Comments)
            {
                target.Comments.Add(comment);
            }

            // Use the earliest start time
            if (source.Started < target.Started)
            {
                target.Started = source.Started;
            }

            // Use the latest finish time (if both are finished)
            if (source.Finished != default && target.Finished != default)
            {
                if (source.Finished > target.Finished)
                {
                    target.Finished = source.Finished;
                }
            }
            else if (source.Finished != default && target.Finished == default)
            {
                target.Finished = source.Finished;
            }

            // Update description if source has one and target doesn't
            if (string.IsNullOrEmpty(target.Description) && !string.IsNullOrEmpty(source.Description))
            {
                target.Description = source.Description;
            }
        }

        public void AddTask(TaskData taskData)
        {
            Guard.AgainstNull(taskData, nameof(taskData));
            string taskIdNormalized = NormalizeTaskId(taskData.Name);
            
            // Check if task already exists globally
            if (RunningTasks.ContainsKey(taskIdNormalized))
            {
                Console.WriteLine($"Task {taskData.Name} already exists.");
                return;
            }
            
            // Set the creator to current developer
            taskData.CreatedBy = _developerConfigController.CurrentDeveloperConfig?.Name ?? "unknown";
            
            // Always save to current developer's file
            SaveTaskToCurrentDeveloper(taskData, true);
            
            // Refresh the aggregated cache
            _runningTasks = null;
        }

        public bool SetTaskAsFinished(TaskData task)
        {
            Guard.AgainstNull(task, nameof(task));
            string taskIdNormalized = NormalizeTaskId(task.Name);
            TaskData? taskData = GetRunningTaskById(taskIdNormalized);
            if (taskData == null)
            {
                _inputAndOutputService.PrintWarning($"Task {task.Name} not found.");
                return false;
            }
            if(taskData!=task)
            {
                _inputAndOutputService.PrintWarning("ERROR: Task Instance not known - not from the system.");
                throw new InvalidOperationException("Task instance not known - not from the system.");
            }
            task.Finished = DateTime.Now;
            
            // Save finished task to current developer's file
            SaveTaskToCurrentDeveloper(task, false);
            
            // Remove from running tasks in current developer's file
            RemoveTaskFromCurrentDeveloper(task, true);
            
            // Refresh the aggregated caches
            _runningTasks = null;
            _finishedTasks = null;
            return true;
        }

        public TaskData? GetRunningTaskById(string taskId)
        {
            string taskIdNormalized = NormalizeTaskId(taskId);
            return RunningTasks.TryGetValue(taskIdNormalized, out TaskData? taskData) ? taskData : null;
        }

        public TaskData? GetGlobalRunningTaskById(string taskId)
        {
            string taskIdNormalized = NormalizeTaskId(taskId);
            return GetAllRunningTasksForAllDevelopers().FirstOrDefault(x=>x.Name.Equals(taskIdNormalized,StringComparison.InvariantCultureIgnoreCase));
        }

        public TaskData? GetFinishedTaskById(string taskId)
        {
            string taskIdNormalized = NormalizeTaskId(taskId);
            return FinishedTasks.TryGetValue(taskIdNormalized, out TaskData? taskData) ? taskData : null;
        }


        public static string NormalizeTaskId(string taskID)
        {
            return taskID.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Save a task to the current developer's file (running or finished)
        /// </summary>
        private void SaveTaskToCurrentDeveloper(TaskData task, bool isRunning)
        {
            if (task == null) return;
            
            string currentDev = _developerConfigController.CurrentDeveloperConfig?.Name ?? "unknown";
            string fileName = isRunning ? GetCurrentDeveloperFileName(true) : GetCurrentDeveloperFileName(false);
            string filePath = GetDateFilePath(fileName);
            
            Dictionary<string, TaskData> tasksForCurrentDev;
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                tasksForCurrentDev = JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions) ?? new Dictionary<string, TaskData>();
            }
            else
            {
                tasksForCurrentDev = new Dictionary<string, TaskData>();
            }
            
            string taskIdNormalized = NormalizeTaskId(task.Name);
            
            // Ensure current developer has entry in the task's work times
            task.EnsureDeveloperEntry(currentDev);
            
            tasksForCurrentDev[taskIdNormalized] = task;
            string jsonOut = JsonSerializer.Serialize(tasksForCurrentDev, _jsonOptions);
            File.WriteAllText(filePath, jsonOut);
            
            if (PublicOptions.IsVerbosLogging)
            {
                _inputAndOutputService.PrintDebug($"Task '{task.Name}' saved to current developer's {(isRunning ? "running" : "finished")} file: {filePath}.");
            }
        }

        /// <summary>
        /// Remove a task from the current developer's file (running or finished)
        /// </summary>
        private void RemoveTaskFromCurrentDeveloper(TaskData task, bool isRunning)
        {
            if (task == null) return;
            
            string fileName = isRunning ? GetCurrentDeveloperFileName(true) : GetCurrentDeveloperFileName(false);
            string filePath = GetDateFilePath(fileName);
            
            if (!File.Exists(filePath)) return;
            
            Dictionary<string, TaskData> tasksForCurrentDev;
            string json = File.ReadAllText(filePath);
            tasksForCurrentDev = JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions) ?? new Dictionary<string, TaskData>();
            
            string taskIdNormalized = NormalizeTaskId(task.Name);
            
            if (tasksForCurrentDev.ContainsKey(taskIdNormalized))
            {
                tasksForCurrentDev.Remove(taskIdNormalized);
                string jsonOut = JsonSerializer.Serialize(tasksForCurrentDev, _jsonOptions);
                File.WriteAllText(filePath, jsonOut);
                
                if (PublicOptions.IsVerbosLogging)
                {
                    _inputAndOutputService.PrintDebug($"Task '{task.Name}' removed from current developer's {(isRunning ? "running" : "finished")} file: {filePath}.");
                }
            }
        }

        // --- New methods for all-developer aggregation ---
        public List<TaskData> GetAllRunningTasksForAllDevelopers()
        {
            return LoadAggregatedTaskData(true).Values.ToList();
        }
        
        public List<TaskData> GetAllFinishedTasksForAllDevelopers()
        {
            return LoadAggregatedTaskData(false).Values.ToList();
        }

        public void AddFocusTimeForTask(string taskId, int finishedMinutes, string developer)
        {
            TaskData? taskData = GetGlobalRunningTaskById(taskId);
            if (taskData == null)
            {
                Console.WriteLine($"Task {taskId} not found.");
                return;
            }
            taskData.AddOrUpdateWorkTime(developer, finishedMinutes);
            
            // Always save to current developer's file
            SaveTaskToCurrentDeveloper(taskData, true);
            
            // Refresh the aggregated cache
            _runningTasks = null;
        }

        public void UpdateTask(TaskData selectedTask)
        {
            Guard.AgainstNull(selectedTask, nameof(selectedTask));
            string taskIdNormalized = NormalizeTaskId(selectedTask.Name);
            
            // Always save to current developer's file
            SaveTaskToCurrentDeveloper(selectedTask, true);
            SaveTaskToCurrentDeveloper(selectedTask, false);
            
            // Refresh the aggregated caches
            _runningTasks = null;
            _finishedTasks = null;
            
            if (PublicOptions.IsVerbosLogging)
            {
                _inputAndOutputService.PrintDebug($"{taskIdNormalized} Task updated and saved to current developer's files.");
            }
        }
    }
}
