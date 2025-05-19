using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;
using System.Text.Json;

namespace DotTimeWork.DataProvider
{
    internal class TaskTimeTrackerDataJson : ITaskTimeTrackerDataProvider
    {
        private Dictionary<string, TaskData> _runningTasks;
        private Dictionary<string, TaskData> _finishedTasks;


        private const string StartTaskDataFileName = "taskStartTimeData.json";
        private const string FinishedTaskDataFileName = "taskFinishedTimeData.json";
        private string _storagePath;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };



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
                    _runningTasks = LoadTaskData(StartTaskDataFileName);
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
                    _finishedTasks = LoadTaskData(FinishedTaskDataFileName);
                }
                return _finishedTasks;
            }
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
                return JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions);
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
                Console.WriteLine($"Task {task.Name} not found.");
                return false;
            }
            if(taskData!=task)
            {
                Console.WriteLine("ERROR: Task Instance not known - not from the system.");
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
                File.WriteAllText(GetDateFilePath(FinishedTaskDataFileName), jsonFinishedTask);
            }
        }

        private void SaveRunningTasks()
        {
            if (_runningTasks != null)
            {
                string jsonStartTask = JsonSerializer.Serialize(_runningTasks, _jsonOptions);
                File.WriteAllText(GetDateFilePath(StartTaskDataFileName), jsonStartTask);
            }
        }

        private string GetDateFilePath(string file)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            return Path.Combine(_storagePath, file);
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
    }
}
