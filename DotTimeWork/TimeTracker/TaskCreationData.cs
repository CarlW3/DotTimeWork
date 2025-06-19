namespace DotTimeWork.TimeTracker
{
    public class TaskCreationData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ExpectedHourCount { get; set; }

        // Optional TaskType property
        public TaskType? TaskType { get; set; }
    }
}
