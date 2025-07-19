using DotTimeWork.DataProvider;
using DotTimeWork.Project;

namespace DotTimeWork.Services
{
    internal class ApplicationInitializationService : IApplicationInitializationService
    {
        private readonly IProjectConfigController _projectConfigController;
        private readonly ITaskTimeTrackerDataProvider _taskTimeTrackerDataProvider;

        public ApplicationInitializationService(IProjectConfigController projectConfigController, ITaskTimeTrackerDataProvider taskTimeTrackerDataProvider)
        {
            _projectConfigController = projectConfigController;
            _taskTimeTrackerDataProvider = taskTimeTrackerDataProvider;
        }

        public void Initialize()
        {
            InitializeTimeTrackingFolder();
        }

        private void InitializeTimeTrackingFolder()
        {
            ProjectConfig foundProject = _projectConfigController.GetCurrentProjectConfig();
            if (foundProject == null)
            {
                Console.WriteLine("No project found.");
                return;
            }
            string timeTrackingFolder = foundProject.TimeTrackingFolder;
            if (string.IsNullOrEmpty(timeTrackingFolder))
            {
                Console.WriteLine("No time tracking folder found.");
                return;
            }
            _taskTimeTrackerDataProvider.SetStoragePath(timeTrackingFolder);
        }
    }
}
