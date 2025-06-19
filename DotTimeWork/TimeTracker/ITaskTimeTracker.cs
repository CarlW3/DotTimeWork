namespace DotTimeWork.TimeTracker
{
    public interface ITaskTimeTracker
    {
        TimeSpan EndTask(string taskId);
        void StartTask(TaskCreationData task);

        List<TaskData> GetAllRunningTasks();
        TaskData GetTaskById(string taskId);
        void AddFocusTimeWork(string taskId, int finishedMinutes, string developer);
        List<TaskData> GetAllFinishedTasks();
        void UpdateTask(TaskData selectedTask);
    }
}