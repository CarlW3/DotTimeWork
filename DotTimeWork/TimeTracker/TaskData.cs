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

        public string Description { get; set; }
        public DateTime Started { get; set; }
        public string Developer { get; set; }

        public int FocusWorkTime { get; set; } = 0;

        /// <summary>
        /// Will be set when Task Finished is called
        /// </summary>
        public DateTime Finished { get; set; }

        public List<TaskComment> Comments { get; set; } = new List<TaskComment>();

        internal void AddComment(TaskComment comment)
        {
            Guard.AgainstNull(comment, nameof(comment));
            Guard.AgainstNullOrEmpty(comment.Comment, nameof(comment));
            Comments.Add(comment);
        }
    }
}
