using DotTimeWork.Commands;
using DotTimeWork.ConsoleService;
using DotTimeWork.DataProvider;
using DotTimeWork.Project;

namespace DotTimeWork.Services
{
    internal class ApplicationInitializationService : IApplicationInitializationService
    {
        private readonly IProjectConfigController _projectConfigController;
        private readonly ITaskTimeTrackerDataProvider _taskTimeTrackerDataProvider;
        private readonly IInputAndOutputService _consoleService;

        public ApplicationInitializationService(
            IProjectConfigController projectConfigController, 
            ITaskTimeTrackerDataProvider taskTimeTrackerDataProvider,
            IInputAndOutputService consoleService)
        {
            _projectConfigController = projectConfigController ?? throw new ArgumentNullException(nameof(projectConfigController));
            _taskTimeTrackerDataProvider = taskTimeTrackerDataProvider ?? throw new ArgumentNullException(nameof(taskTimeTrackerDataProvider));
            _consoleService = consoleService ?? throw new ArgumentNullException(nameof(consoleService));
        }

        public void Initialize()
        {
            try
            {
                InitializeTimeTrackingFolder();
            }
            catch (Exception ex)
            {
                _consoleService.PrintError($"Failed to initialize application: {ex.Message}");
                throw;
            }
        }

        private void InitializeTimeTrackingFolder()
        {
            var projectConfig = _projectConfigController.GetCurrentProjectConfig();
            if (projectConfig?.TimeTrackingFolder == null)
            {
                _consoleService.PrintWarning("No project or time tracking folder configured. Some features may not work properly.");
                return;
            }

            _taskTimeTrackerDataProvider.SetStoragePath(projectConfig.TimeTrackingFolder);
            
            if (PublicOptions.IsVerbosLogging)
            {
                _consoleService.PrintSuccess($"Time tracking folder initialized: {projectConfig.TimeTrackingFolder}");
            }
        }
    }
}
