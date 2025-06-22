using DotTimeWork.Helper;

namespace DotTimeWork.TimeTracker
{
    public class TaskComment
    {
        public DateTime Created { get; set; }
        public string Developer { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }

    public class TaskData
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime Started { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        /// <summary>
        /// Per-developer working time in minutes
        /// </summary>
        public Dictionary<string, int> DeveloperWorkTimes { get; set; } = new Dictionary<string, int>();
        /// <summary>
        /// Will be set when Task Finished is called
        /// </summary>
        public DateTime Finished { get; set; }
        public List<TaskComment> Comments { get; set; } = new List<TaskComment>();

        /// <summary>
        /// Add or update working time for a developer
        /// </summary>
        public void AddOrUpdateWorkTime(string developer, int minutes)
        {
            if (string.IsNullOrWhiteSpace(developer)) return;
            if (DeveloperWorkTimes.ContainsKey(developer))
                DeveloperWorkTimes[developer] += minutes;
            else
                DeveloperWorkTimes[developer] = minutes;
        }

        /// <summary>
        /// Ensure a developer is present in the dictionary, with 0 if not yet worked
        /// </summary>
        public void EnsureDeveloperEntry(string developer)
        {
            if (!string.IsNullOrWhiteSpace(developer) && !DeveloperWorkTimes.ContainsKey(developer))
                DeveloperWorkTimes[developer] = 0;
        }

        /// <summary>
        /// Get total working time for all developers
        /// </summary>
        public int GetTotalWorkTime() => DeveloperWorkTimes.Values.Sum();

        /// <summary>
        /// Get working time for a specific developer
        /// </summary>
        public int GetWorkTimeForDeveloper(string developer)
        {
            if (DeveloperWorkTimes.TryGetValue(developer, out int min))
                return min;
            return 0;
        }

        internal void AddComment(TaskComment comment)
        {
            Guard.AgainstNull(comment, nameof(comment));
            Guard.AgainstNullOrEmpty(comment.Comment, nameof(comment));
            Comments.Add(comment);
        }
    }
}
