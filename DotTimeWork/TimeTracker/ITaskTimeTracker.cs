namespace DotTimeWork.TimeTracker
{
    public interface ITaskTimeTracker
    {
        TimeSpan EndTask(string taskId);
        void StartTask(TaskCreationData task);

        List<TaskData> GetAllRunningTasks();
        TaskData? GetTaskById(string taskId);
        /// <summary>
        /// Find Task from all developers, not only the current one
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        TaskData? GetGlobalRunningTaskById(string taskId);
        void AddFocusTimeWork(string taskId, int finishedMinutes, string developer);
        List<TaskData> GetAllFinishedTasks();
        void UpdateTask(TaskData selectedTask);
        /// <summary>
        /// Checks if a task is assigned to the current developer
        /// </summary>
        /// <param name="taskId">ID of the task to check</param>
        /// <returns>True if the task is assigned to the current developer</returns>
        bool IsTaskAssignedToCurrentDeveloper(string taskId);
    }
}