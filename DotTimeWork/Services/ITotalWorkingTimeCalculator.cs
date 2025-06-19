
namespace DotTimeWork.Services
{
    public interface ITotalWorkingTimeCalculator
    {
        TimeSpan GetTotalTimeFinishedTasks();
        TimeSpan GetTotalTimeRunningTasks();
        TimeSpan GetWorkingSpanTime();
        void LoadTasks();

        int TotalMinutesFocusWorkingTime { get; }
    }
}