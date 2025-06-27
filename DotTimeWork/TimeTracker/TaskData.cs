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
        public required string Name { get; set; }
        public string? Description { get; set; }
        /// <summary>
        /// When the task was originally created
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Per-developer start times
        /// </summary>
        public Dictionary<string, DateTime> DeveloperStartTimes { get; set; } = new Dictionary<string, DateTime>();
        /// <summary>
        /// Legacy property - use DeveloperStartTimes[CreatedBy] instead for new code
        /// </summary>
        public DateTime Started 
        { 
            get => DeveloperStartTimes.ContainsKey(CreatedBy) ? DeveloperStartTimes[CreatedBy] : Created;
            set 
            { 
                if (!DeveloperStartTimes.ContainsKey(CreatedBy))
                    DeveloperStartTimes[CreatedBy] = value;
            }
        }
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

        /// <summary>
        /// Get start time for a specific developer
        /// </summary>
        public DateTime GetDeveloperStartTime(string developer)
        {
            if (string.IsNullOrWhiteSpace(developer)) return DateTime.MinValue;
            if (DeveloperStartTimes.ContainsKey(developer))
                return DeveloperStartTimes[developer];
            return DateTime.MinValue;
        }

        /// <summary>
        /// Set start time for a developer
        /// </summary>
        public void SetDeveloperStartTime(string developer, DateTime startTime)
        {
            if (string.IsNullOrWhiteSpace(developer)) return;
            DeveloperStartTimes[developer] = startTime;
        }

        /// <summary>
        /// Check if a developer has already started this task
        /// </summary>
        public bool IsDeveloperParticipating(string developer)
        {
            if (string.IsNullOrWhiteSpace(developer))
                return false;
                
            // Check both dictionaries to ensure robust detection
            return DeveloperStartTimes.ContainsKey(developer) || DeveloperWorkTimes.ContainsKey(developer);
        }

        internal void AddComment(TaskComment comment)
        {
            Guard.AgainstNull(comment, nameof(comment));
            Guard.AgainstNullOrEmpty(comment.Comment, nameof(comment));
            Comments.Add(comment);
        }
    }
}
