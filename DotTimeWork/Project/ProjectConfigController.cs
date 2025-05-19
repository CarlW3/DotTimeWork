using DotTimeWork.Commands;
using DotTimeWork.ConsoleService;
using DotTimeWork.DataProvider;
using System.Runtime.Versioning;

namespace DotTimeWork.Project
{
    /// <summary>
    /// This should by DI and singlton
    /// </summary>
    public class ProjectConfigController : IProjectConfigController
    {
        private ProjectConfig? _currentProjectConfig;
        private readonly IProjectConfigDataProvider _projectConfigDataProvider;
        private readonly IInputAndOutputService _inputAndOutputService;

        public ProjectConfigController(IProjectConfigDataProvider projectConfigDataProvider, IInputAndOutputService inputAndOutputService)
        {
            _inputAndOutputService = inputAndOutputService;
            _projectConfigDataProvider = projectConfigDataProvider;
        }

        public void CreateProjectConfigFile()
        {
            if (_projectConfigDataProvider.ProjectConfigFileExists())
            {
                _inputAndOutputService.PrintNormal("Project config file already exists. Deleting it.");
                _projectConfigDataProvider.DeleteProjectConfigFile();
            }
            _inputAndOutputService.PrintNormal("Creating project config file...");

            var projectName = _inputAndOutputService.AskForInput(Properties.Resources.Project_Ask_Name,"No name");

            var projectDescription = _inputAndOutputService.AskForInput(Properties.Resources.Project_Ask_Description,string.Empty);

            var projectStartDate = _inputAndOutputService.AskForInput(Properties.Resources.Project_Ask_StartDate,DateTime.Now.ToString("yyyy-MM-dd"));
            
            var projectEndDate = _inputAndOutputService.AskForInput(Properties.Resources.Project_Ask_EndDate, "N/A");

            var maxTime = _inputAndOutputService.AskForInput(Properties.Resources.Project_Ask_TimePerDay,0);


            DateTime projectStart;

            if (string.IsNullOrWhiteSpace(projectStartDate))
            {
                projectStart = DateTime.Now;
            }
            else if (!DateTime.TryParse(projectStartDate, out projectStart))
            {
                _inputAndOutputService.PrintNormal("Invalid project start date. Using current date.");
                projectStart = DateTime.Now;
            }

            DateTime projectEnd;
            if(string.IsNullOrEmpty(projectEndDate))
            {
                projectEnd = DateTime.MinValue;
            }
            else if (!DateTime.TryParse(projectEndDate, out projectEnd))
            {
                _inputAndOutputService.PrintNormal("Invalid project end date. Using current date + 30 days.");
                projectEnd = DateTime.Now.AddDays(30);
            }

            _currentProjectConfig = new ProjectConfig
            {
                ProjectName = projectName,
                Description = projectDescription,
                MaxTimePerDay = maxTime,
                ProjectStart = projectStart,
                ProjectEnd = projectEnd == DateTime.MinValue ? null : projectEnd,
            };

            _projectConfigDataProvider.PersistProjectConfig(_currentProjectConfig);

            _inputAndOutputService.PrintSuccess("Project config file created.");

        }

        public ProjectConfig GetCurrentProjectConfig()
        {
            if (_currentProjectConfig == null)
            {
                if (PublicOptions.IsVerbosLogging)
                {
                    _inputAndOutputService.PrintNormal("Project config file not loaded. Loading now...");
                }
                _currentProjectConfig=_projectConfigDataProvider.LoadProjectConfig();
            }
            return _currentProjectConfig;
        }
    }
}
