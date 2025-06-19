namespace DotTimeWork.TimeTracker
{
    /// <summary>
    /// Represents the type of work associated with a task.
    /// </summary>
    public enum TaskType
    {
        /// <summary>
        /// Tasks related to analyzing requirements, problems, or solutions.
        /// </summary>
        Analysis,

        /// <summary>
        /// Tasks focused on creating or maintaining documentation.
        /// </summary>
        Documentation,

        /// <summary>
        /// Tasks involving planning, scheduling, or organizing work.
        /// </summary>
        Planning,

        /// <summary>
        /// Tasks that involve writing, reviewing, or maintaining code.
        /// </summary>
        Programming,

        /// <summary>
        /// Tasks that do not fit into the other defined categories.
        /// </summary>
        Other
    }
}