
namespace DotTimeWork.Services
{
    public interface ITotalWorkingTimeCalculator
    {
        TimeSpan GetTotalTimeFinishedTasks();
        TimeSpan GetTotalTimeRunningTasks();
        TimeSpan GetWorkingSpanTime();

        int TotalMinutesFocusWorkingTime { get; }
    }
}