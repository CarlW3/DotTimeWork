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
                    _runningTasks = LoadTaskData(GetCurrentDeveloperFileName(true));
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
                    _finishedTasks = LoadTaskData(GetCurrentDeveloperFileName(false));
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

        public void AddTask(TaskData taskData)
        {
            Guard.AgainstNull(taskData, nameof(taskData));
            string taskIdNormalized = NormalizeTaskId(taskData.Name);
            if (RunningTasks.ContainsKey(taskIdNormalized))
            {
                Console.WriteLine($"Task {taskData.Name} already exists.");
                return;
            }
            RunningTasks.Add(taskIdNormalized, taskData);
            SaveRunningTasks();
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
            FinishedTasks.Add(taskIdNormalized, task);
            RunningTasks.Remove(taskIdNormalized);
            SaveAllTaskData();
            return true;
        }

        public TaskData? GetRunningTaskById(string taskId)
        {
            string taskIdNormalized = NormalizeTaskId(taskId);
            return RunningTasks.TryGetValue(taskIdNormalized, out TaskData? taskData) ? taskData : null;
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

        private void SaveAllTaskData()
        {
            SaveRunningTasks();
            SaveFinishedTasks();
        }

        private void SaveFinishedTasks()
        {
            if (_finishedTasks != null)
            {
                string jsonFinishedTask = JsonSerializer.Serialize(_finishedTasks, _jsonOptions);
                File.WriteAllText(GetDateFilePath(GetCurrentDeveloperFileName(false)), jsonFinishedTask);
                if(PublicOptions.IsVerbosLogging)
                {
                    _inputAndOutputService.PrintDebug($"Finished tasks saved to {GetDateFilePath(GetCurrentDeveloperFileName(false))}.");
                }
            }
        }

        private void SaveRunningTasks()
        {
            if (_runningTasks != null)
            {
                string jsonStartTask = JsonSerializer.Serialize(_runningTasks, _jsonOptions);
                File.WriteAllText(GetDateFilePath(GetCurrentDeveloperFileName(true)), jsonStartTask);
                if (PublicOptions.IsVerbosLogging)
                {
                    _inputAndOutputService.PrintDebug($"Running tasks saved to {GetDateFilePath(GetCurrentDeveloperFileName(true))}.");
                }
            }
        }

        // --- New methods for all-developer aggregation ---
        public List<TaskData> GetAllRunningTasksForAllDevelopers()
        {
            return GetAllTasksForAllDevelopers(true);
        }
        public List<TaskData> GetAllFinishedTasksForAllDevelopers()
        {
            return GetAllTasksForAllDevelopers(false);
        }
        private List<TaskData> GetAllTasksForAllDevelopers(bool running)
        {
            var result = new List<TaskData>();
            if (string.IsNullOrEmpty(_storagePath) || !Directory.Exists(_storagePath))
                return result;
            var pattern = running ? "taskStartTimeData.*.json" : "taskFinishedTimeData.*.json";
            foreach (var file in Directory.GetFiles(_storagePath, pattern))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions);
                    if (dict != null)
                        result.AddRange(dict.Values);
                }
                catch { }
            }
            return result;
        }

        public void AddFocusTimeForTask(string taskId, int finishedMinutes)
        {
            TaskData? taskData = GetRunningTaskById(taskId);
            if (taskData == null)
            {
                Console.WriteLine($"Task {taskId} not found.");
                return;
            }
            taskData.FocusWorkTime += finishedMinutes;
            SaveRunningTasks();
        }

        public void UpdateTask(TaskData selectedTask)
        {
            Guard.AgainstNull(selectedTask, nameof(selectedTask));
            string taskIdNormalized = NormalizeTaskId(selectedTask.Name);
            bool taskUpdated=false;
            if (RunningTasks.ContainsKey(taskIdNormalized))
            {
                RunningTasks[taskIdNormalized] = selectedTask;
                SaveRunningTasks();
                taskUpdated = true;
            }
            if(FinishedTasks.ContainsKey(taskIdNormalized))
            {
                FinishedTasks[taskIdNormalized] = selectedTask;
                SaveFinishedTasks();
                taskUpdated = true;
            }
            if (PublicOptions.IsVerbosLogging)
            {
                _inputAndOutputService.PrintDebug(taskIdNormalized + " Task Update status: " + (taskUpdated ? "OK" : "Failed - Task not found."));
            }
        }
    }
}
