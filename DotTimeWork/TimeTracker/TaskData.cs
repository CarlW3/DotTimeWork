using DotTimeWork.Helper;

namespace DotTimeWork.TimeTracker
{
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

        public List<string> Comments { get; set; } = new List<string>();

        internal void AddComment(string comment)
        {
            Guard.AgainstNullOrEmpty(comment, nameof(comment));
            Comments.Add(comment);
        }
    }
}
