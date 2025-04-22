using DotTimeWork.Developer;
using DotTimeWork.Project;
using System.Text.Json;

namespace DotTimeWork.TimeTracker
{
    internal class TaskTimeTracker : ITaskTimeTracker
    {
        private const string StartTaskDataFileName = "taskStartTimeData.json";
        private const string FinishedTaskDataFileName = "taskFinishedTimeData.json";

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };

        private Dictionary<string, TaskData> _taskStartTimes;
        private Dictionary<string, TaskData> _finishedTasks;
        private readonly IProjectConfigController _projectConfigController;
        private readonly IDeveloperConfigController _developerConfigController;

        public TaskTimeTracker(IProjectConfigController projectConfigController, IDeveloperConfigController developerConfigController)
        {
            _projectConfigController = projectConfigController;
            _developerConfigController = developerConfigController;
            _taskStartTimes = LoadTaskData(StartTaskDataFileName);
            _finishedTasks = LoadTaskData(FinishedTaskDataFileName);
        }

        public void StartTask(TaskCreationData creationData)
        {
            var currentDeveloper = _developerConfigController.CurrentDeveloperConfig;
            if (_taskStartTimes.ContainsKey(creationData.Name))
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
            _taskStartTimes[Normalize(creationData.Name)] = newTask;
            SaveTaskData();
            Console.WriteLine($"Task {newTask.Name} started at {newTask.Started}.");
        }

        public TimeSpan EndTask(string taskId)
        {
            string taskIdNormalized = Normalize(taskId);
            if (!_taskStartTimes.TryGetValue(taskIdNormalized, out TaskData? foundTask))
            {
                Console.WriteLine($"Task {taskIdNormalized} has not been started.");
                return TimeSpan.Zero;
            }
            foundTask.Finished = DateTime.Now;
            TimeSpan duration = foundTask.Finished - foundTask.Started;

            // Move task to the finished list:
            _taskStartTimes.Remove(taskIdNormalized);
            _finishedTasks.Add(taskIdNormalized, foundTask);

            SaveTaskData();
            Console.WriteLine($"Task {taskIdNormalized} ended at {foundTask.Finished}. Duration: {duration}.");

            return duration;
        }

        public TaskData GetTaskById(string taskId)
        {
            string taskIdNormalized = Normalize(taskId);
            if (!_taskStartTimes.TryGetValue(taskIdNormalized, out TaskData? value))
            {
                Console.WriteLine($"Task {taskIdNormalized} not found.");
                return null;
            }
            return value;
        }

        public void AddFocusTimeWork(string taskId,int finishedMinutes)
        {
            string taskIdNormalized = Normalize(taskId);
            TaskData foundTask = GetTaskById(taskIdNormalized);
            if (foundTask!=null)
            {
                foundTask.FocusWorkTime += finishedMinutes;
                SaveTaskData();
            }
        }
        private Dictionary<string, TaskData> LoadTaskData(string file)
        {
            if (!File.Exists(GetDateFilePath(file)))
            {
                return new Dictionary<string, TaskData>();
            }
            string json = File.ReadAllText(GetDateFilePath(file));
            return JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, _jsonOptions);
        }

        private void SaveTaskData()
        {
            string jsonStartTask = JsonSerializer.Serialize(_taskStartTimes, _jsonOptions);
            File.WriteAllText(GetDateFilePath(StartTaskDataFileName), jsonStartTask);
            string jsonFinishedTask = JsonSerializer.Serialize(_finishedTasks, _jsonOptions);
            File.WriteAllText(GetDateFilePath(FinishedTaskDataFileName), jsonFinishedTask);
        }

        public List<TaskData> GetAllTasks()
        {
            _taskStartTimes = LoadTaskData(StartTaskDataFileName);

            return _taskStartTimes.Values.ToList();
        }

        public List<TaskData> GetAllFinishedTasks()
        {
            return LoadTaskData(FinishedTaskDataFileName).Values.ToList();
        }


        private string GetDateFilePath(string file)
        {
            ProjectConfig config = _projectConfigController.GetCurrentProjectConfig();
            if(!Directory.Exists(config.TimeTrackingFolder))
            {
                Directory.CreateDirectory(config.TimeTrackingFolder);
            }
            return Path.Combine(config.TimeTrackingFolder, file);
        }

        private static string Normalize(string taskID)
        {
            return taskID.Trim().ToLowerInvariant();
        }
    }
}
