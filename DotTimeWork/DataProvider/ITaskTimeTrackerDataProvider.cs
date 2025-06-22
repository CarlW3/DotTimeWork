using DotTimeWork.TimeTracker;

namespace DotTimeWork.DataProvider
{
    public interface ITaskTimeTrackerDataProvider
    {
        Dictionary<string, TaskData> FinishedTasks { get; }
        Dictionary<string, TaskData> RunningTasks { get; }
        void AddFocusTimeForTask(string taskId, int finishedMinutes, string developer);
        void AddTask(TaskData taskData);
        TaskData? GetFinishedTaskById(string taskId);
        TaskData? GetRunningTaskById(string taskId);
        void SetStoragePath(string path);
        bool SetTaskAsFinished(TaskData task);
        void UpdateTask(TaskData selectedTask);
        List<TaskData> GetAllRunningTasksForAllDevelopers();
        List<TaskData> GetAllFinishedTasksForAllDevelopers();
        TaskData? GetGlobalRunningTaskById(string taskId);
    }
}