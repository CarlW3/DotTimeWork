using DotTimeWork.Project;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class CreateProjectCommand:Command
    {
        private readonly IProjectConfigController _projectConfigController;
        public CreateProjectCommand(IProjectConfigController projectConfigController) : base("CreateProject", "Creates Project Config file")
        {
            _projectConfigController = projectConfigController;
            this.SetHandler(Execute,PublicOptions.VerboseLogging);
            Description = "Creates Project Config file. This will create a new project config file in the current directory. The config file will be used to store the project settings and tasks.";
        }

        private void Execute(bool verboseLogging)
        {
            PublicOptions.IsVerbosLogging = verboseLogging;
            if (verboseLogging)
            {
                AnsiConsole.MarkupLine($"[grey]Project File creation started....[/]");
            }
            _projectConfigController.CreateProjectConfigFile();
        }
    }
}
