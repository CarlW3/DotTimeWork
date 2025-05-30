﻿using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal static class PublicOptions
    {
        public static Option<string> TaskIdOption = new Option<string>("--Task", "The name or ID of the task")
        {
            Description = "The name or ID of the task. If not specified, user can enter manually"
        };

        public static Option<string> OutputFile = new Option<string>("--output", "The output file path")
        {
            IsRequired = false,
            ArgumentHelpName = "output",
            Description = "The output file path. If not specified, the default path will be used."
        };

        public static Option<bool> OpenReportAfterCreate= new Option<bool>("--open",()=>true, "Open the report after creation")
        {
            IsRequired = false,
            ArgumentHelpName = "open",
            Description = "Open the report after creation. If not specified, the default behavior will be used."
        };

        public static Option<bool> ReportIncludeFinishedTasks = new Option<bool>("--include-finished",()=>true, "Include finished tasks in the report")
        {
            IsRequired = false,
            ArgumentHelpName = "include-finished",
            Description = "Include finished tasks in the report. If not specified, the default behavior will be used."
        };

        public static Option<bool> ReportIncludeComments = new Option<bool>("--include-comments", () => true, "Include task comments in the report")
        {
            IsRequired = false,
            ArgumentHelpName = "include-comments",
            Description = "Include Comments added to the Task. If not specified, the default behavior will be used."
        };

        public static Option<string> CommentText = new Option<string>("--comment", "The comment text")
        {
            IsRequired = false,
            ArgumentHelpName = "comment",
            Description = "The comment text. If not specified, the comment needs to be entered during execution"
        };

        public static Option<bool> VerboseLogging=new Option<bool>("--verbose", "Enable verbose logging")
        {
            IsRequired = false,
            ArgumentHelpName = "verbose",
        };

        public static void InitOptions()
        {
            VerboseLogging.AddAlias("-v");
            TaskIdOption.AddAlias("-t");
        }
        /// <summary>
        /// Can be activated by Command Line option to allow more verbose logging
        /// </summary>
        public static bool IsVerbosLogging { get; set; }
    }
}
