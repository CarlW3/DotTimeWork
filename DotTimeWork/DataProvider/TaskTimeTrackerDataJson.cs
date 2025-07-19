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


        /// <summary>
        /// Load and aggregate task data from all developers
        /// </summary>
        private Dictionary<string, TaskData> LoadAggregatedTaskData(bool running)
        {
            var aggregatedTasks = new Dictionary<string, TaskData>(StringComparer.OrdinalIgnoreCase);
            
            if (string.IsNullOrEmpty(_storagePath) || !Directory.Exists(_storagePath))
                return aggregatedTasks;

            var pattern = running ? "taskStartTimeData.*.json" : "taskFinishedTimeData.*.json";
            var files = Directory.GetFiles(_storagePath, pattern);
            
            if (files.Length == 0)
                return aggregatedTasks;

            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Length == 0) continue; // Skip empty files
                    
                    string json = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(json)) continue;
                    
                    var dict = JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions);
                    if (dict?.Count > 0)
                    {
                        foreach (var (key, taskFromFile) in dict)
                        {
                            string normalizedTaskId = NormalizeTaskId(key);
                            
                            if (aggregatedTasks.TryGetValue(normalizedTaskId, out TaskData? existingTask))
                            {
                                // Merge with existing task
                                MergeTaskData(existingTask, taskFromFile);
                            }
                            else
                            {
                                // Add new task (clone to avoid reference issues)
                                aggregatedTasks[normalizedTaskId] = CloneTaskData(taskFromFile);
                            }
                        }
                    }
                }
                catch (Exception ex) when (PublicOptions.IsVerbosLogging)
                {
                    _inputAndOutputService.PrintDebug($"Error reading file {file}: {ex.Message}");
                }
                catch
                {
                    // Silently ignore files that can't be read in non-verbose mode
                }
            }
            
            return aggregatedTasks;
        }

        /// <summary>
        /// Create a copy of TaskData with only comments from the specified developer
        /// </summary>
        private static TaskData CloneTaskForCurrentDeveloper(TaskData source, string currentDeveloper)
        {
            return new TaskData
            {
                Name = source.Name,
                Description = source.Description,
                Created = source.Created,
                Finished = source.Finished,
                FinishedBy = source.FinishedBy, // Include who finished the task
                CreatedBy = source.CreatedBy,
                DeveloperStartTimes = new Dictionary<string, DateTime>(source.DeveloperStartTimes),
                DeveloperWorkTimes = new Dictionary<string, int>(source.DeveloperWorkTimes),
                // Copy active developers list
                ActiveDevelopers = new HashSet<string>(source.ActiveDevelopers ?? new HashSet<string>()),
                // Only include comments from the current developer
                Comments = new List<TaskComment>(
                    source.Comments
                        .Where(c => c.Developer.Equals(currentDeveloper, StringComparison.OrdinalIgnoreCase))
                        .Select(c => new TaskComment
                        {
                            Created = c.Created,
                            Developer = c.Developer,
                            Comment = c.Comment
                        })
                )
            };
        }

        /// <summary>
        /// Create a deep copy of TaskData to avoid reference issues during aggregation
        /// </summary>
        private static TaskData CloneTaskData(TaskData source)
        {
            return new TaskData
            {
                Name = source.Name,
                Description = source.Description,
                Created = source.Created, 
                Finished = source.Finished,
                FinishedBy = source.FinishedBy, // Include who finished the task
                CreatedBy = source.CreatedBy,
                DeveloperStartTimes = new Dictionary<string, DateTime>(source.DeveloperStartTimes),
                DeveloperWorkTimes = new Dictionary<string, int>(source.DeveloperWorkTimes),
                // Copy active developers list
                ActiveDevelopers = new HashSet<string>(source.ActiveDevelopers ?? new HashSet<string>()),
                Comments = new List<TaskComment>(source.Comments.Select(c => new TaskComment
                {
                    Created = c.Created,
                    Developer = c.Developer,
                    Comment = c.Comment
                }))
            };
        }

        /// <summary>
        /// Merge task data from different developers working on the same task
        /// </summary>
        private static void MergeTaskData(TaskData target, TaskData source)
        {
            // Merge developer work times
            foreach (var (developer, minutes) in source.DeveloperWorkTimes)
            {
                target.DeveloperWorkTimes[developer] = target.DeveloperWorkTimes.GetValueOrDefault(developer, 0) + minutes;
            }

            // Merge comments (avoid duplicates by checking timestamp and developer)
            var existingCommentKeys = new HashSet<string>(
                target.Comments.Select(c => $"{c.Created:yyyy-MM-dd HH:mm:ss}_{c.Developer}_{c.Comment}")
            );
            
            foreach (var comment in source.Comments)
            {
                string commentKey = $"{comment.Created:yyyy-MM-dd HH:mm:ss}_{comment.Developer}_{comment.Comment}";
                if (!existingCommentKeys.Contains(commentKey))
                {
                    target.Comments.Add(new TaskComment
                    {
                        Created = comment.Created,
                        Developer = comment.Developer,
                        Comment = comment.Comment
                    });
                    existingCommentKeys.Add(commentKey);
                }
            }

            // Merge developer start times, keeping the earliest time for each developer
            foreach (var devStartTime in source.DeveloperStartTimes)
            {
                if (!target.DeveloperStartTimes.ContainsKey(devStartTime.Key) || 
                    devStartTime.Value < target.DeveloperStartTimes[devStartTime.Key])
                {
                    target.DeveloperStartTimes[devStartTime.Key] = devStartTime.Value;
                }
            }
            
            // Merge active developers list
            if (source.ActiveDevelopers != null)
            {
                if (target.ActiveDevelopers == null)
                {
                    target.ActiveDevelopers = new HashSet<string>(source.ActiveDevelopers);
                }
                else
                {
                    foreach (var dev in source.ActiveDevelopers)
                    {
                        target.ActiveDevelopers.Add(dev);
                    }
                }
            }
            
            // Ensure Created property has the earliest time between the two tasks
            if (source.Created < target.Created)
            {
                target.Created = source.Created;
            }

            // Use the latest finish time (if both are finished)
            if (source.Finished != default && target.Finished != default)
            {
                if (source.Finished > target.Finished)
                {
                    target.Finished = source.Finished;
                    // If finish time is updated, update the developer who finished it
                    if (!string.IsNullOrEmpty(source.FinishedBy))
                    {
                        target.FinishedBy = source.FinishedBy;
                    }
                }
            }
            else if (source.Finished != default && target.Finished == default)
            {
                target.Finished = source.Finished;
                target.FinishedBy = source.FinishedBy;
            }

            // Update description if source has one and target doesn't
            if (string.IsNullOrEmpty(target.Description) && !string.IsNullOrEmpty(source.Description))
            {
                target.Description = source.Description;
            }

            // Ensure the created by field reflects the earliest creator
            if (string.IsNullOrEmpty(target.CreatedBy) && !string.IsNullOrEmpty(source.CreatedBy))
            {
                target.CreatedBy = source.CreatedBy;
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
            
            // Set finish time and developer who finished it
            task.Finished = DateTime.Now;
            string currentDeveloper = _developerConfigController.CurrentDeveloperConfig?.Name ?? "unknown";
            task.FinishedBy = currentDeveloper;
            
            // Remove current developer from active developers list
            if (task.ActiveDevelopers.Contains(currentDeveloper))
            {
                task.ActiveDevelopers.Remove(currentDeveloper);
            }
            
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
        /// Only saves comments from the current developer
        /// </summary>
        private void SaveTaskToCurrentDeveloper(TaskData task, bool isRunning)
        {
            if (task == null) return;
            
            string currentDev = _developerConfigController.CurrentDeveloperConfig?.Name ?? "unknown";
            string fileName = isRunning ? GetCurrentDeveloperFileName(true) : GetCurrentDeveloperFileName(false);
            string filePath = GetDateFilePath(fileName);
            
            var tasksForCurrentDev = LoadTasksFromFile(filePath);
            string taskIdNormalized = NormalizeTaskId(task.Name);
            
            // Create a copy of the task with only current developer's comments
            var taskForCurrentDev = CloneTaskForCurrentDeveloper(task, currentDev);
            
            // Ensure current developer has entry in the task's work times
            taskForCurrentDev.EnsureDeveloperEntry(currentDev);
            
            tasksForCurrentDev[taskIdNormalized] = taskForCurrentDev;
            SaveTasksToFile(filePath, tasksForCurrentDev);
            
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
            
            var tasksForCurrentDev = LoadTasksFromFile(filePath);
            string taskIdNormalized = NormalizeTaskId(task.Name);
            
            if (tasksForCurrentDev.Remove(taskIdNormalized))
            {
                SaveTasksToFile(filePath, tasksForCurrentDev);
                
                if (PublicOptions.IsVerbosLogging)
                {
                    _inputAndOutputService.PrintDebug($"Task '{task.Name}' removed from current developer's {(isRunning ? "running" : "finished")} file: {filePath}.");
                }
            }
        }

        /// <summary>
        /// Load tasks from a file, return empty dictionary if file doesn't exist or can't be read
        /// </summary>
        private static Dictionary<string, TaskData> LoadTasksFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new Dictionary<string, TaskData>(StringComparer.OrdinalIgnoreCase);

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions) 
                       ?? new Dictionary<string, TaskData>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, TaskData>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Save tasks to a file
        /// </summary>
        private static void SaveTasksToFile(string filePath, Dictionary<string, TaskData> tasks)
        {
            string jsonOut = JsonSerializer.Serialize(tasks, _jsonOptions);
            File.WriteAllText(filePath, jsonOut);
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
