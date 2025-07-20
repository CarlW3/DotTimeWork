using System.CommandLine;

namespace DotTimeWork.Commands
{
    /// <summary>
    /// Global command line options used across multiple commands
    /// </summary>
    internal static class PublicOptions
    {
        public static readonly Option<string> TaskIdOption = new("--Task", "The name or ID of the task")
        {
            Description = "The name or ID of the task. If not specified, user can enter manually"
        };

        public static readonly Option<string> OutputFile = new("--output", "The output file path")
        {
            IsRequired = false,
            ArgumentHelpName = "output",
            Description = "The output file path. If not specified, the default path will be used."
        };

        public static readonly Option<bool> OpenReportAfterCreate = new("--open", () => true, "Open the report after creation")
        {
            IsRequired = false,
            ArgumentHelpName = "open",
            Description = "Open the report after creation. If not specified, the default behavior will be used."
        };

        public static readonly Option<bool> ReportIncludeFinishedTasks = new("--include-finished", () => true, "Include finished tasks in the report")
        {
            IsRequired = false,
            ArgumentHelpName = "include-finished",
            Description = "Include finished tasks in the report. If not specified, the default behavior will be used."
        };

        public static readonly Option<bool> ReportIncludeComments = new("--include-comments", () => true, "Include task comments in the report")
        {
            IsRequired = false,
            ArgumentHelpName = "include-comments",
            Description = "Include Comments added to the Task. If not specified, the default behavior will be used."
        };

        public static readonly Option<string> CommentText = new("--comment", "The comment text")
        {
            IsRequired = false,
            ArgumentHelpName = "comment",
            Description = "The comment text. If not specified, the comment needs to be entered during execution"
        };

        public static readonly Option<bool> VerboseLogging = new("--verbose", "Enable verbose logging")
        {
            IsRequired = false,
            ArgumentHelpName = "verbose",
            Description = "Enable verbose logging for debugging purposes"
        };

        /// <summary>
        /// Runtime flag for verbose logging state
        /// </summary>
        public static bool IsVerbosLogging { get; set; }

        /// <summary>
        /// Initialize option aliases
        /// </summary>
        public static void InitializeAliases()
        {
            VerboseLogging.AddAlias("-v");
            TaskIdOption.AddAlias("-t");
            OutputFile.AddAlias("-o");
        }
    }
}
