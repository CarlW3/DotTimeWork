using DotTimeWork.ConsoleService;
using DotTimeWork.Project;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class CreateProjectCommand:Command
    {
        private readonly IProjectConfigController _projectConfigController;
        private readonly IInputAndOutputService _inputAndOutputService;
        public CreateProjectCommand(IProjectConfigController projectConfigController,IInputAndOutputService inputAndOutputService) : base("CreateProject", "Creates Project Config file")
        {
            _projectConfigController = projectConfigController;
            _inputAndOutputService = inputAndOutputService;
            this.SetHandler(Execute,PublicOptions.VerboseLogging);
            Description = "Creates Project Config file. This will create a new project config file in the current directory. The config file will be used to store the project settings and tasks.";
        }

        internal void Execute(bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (verboseLogging)
            {
                _inputAndOutputService.PrintDebug($"Project File creation started....");
            }
            _projectConfigController.CreateProjectConfigFile();
            _inputAndOutputService.PrintSuccess("Project config file created successfully.");
        }
    }
}
