namespace DotTimeWork.Project
{
    public class ProjectConfig
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public int MaxTimePerDay { get; set; }
        public DateTime ProjectStart { get; set; }
        public DateTime? ProjectEnd { get; set; }

        public string TimeTrackingFolder { get; set; } = "DotTimeWork";

    }
}
