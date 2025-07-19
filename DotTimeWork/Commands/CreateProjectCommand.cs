using DotTimeWork.Commands.Base;
using DotTimeWork.Project;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class CreateProjectCommand : BaseCommand
    {
        private readonly IProjectConfigController _projectConfigController;

        public CreateProjectCommand(IProjectConfigController projectConfigController) 
            : base("CreateProject", "Creates Project Config file")
        {
            _projectConfigController = projectConfigController ?? throw new ArgumentNullException(nameof(projectConfigController));
            Description = "Creates Project Config file. This will create a new project config file in the current directory. The config file will be used to store the project settings and tasks.";
        }

        protected override void SetupCommand()
        {
            this.SetHandler(Execute, PublicOptions.VerboseLogging);
        }

        private void Execute(bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                if (verboseLogging)
                {
                    Console.PrintDebug("Project File creation started....");
                }

                CheckExistingProjectConfig(verboseLogging);
                CreateProjectConfig();
                
                Console.PrintSuccess("Project config file created successfully.");
                
                if (verboseLogging)
                {
                    Console.PrintDebug("Project configuration setup completed.");
                }
            }, verboseLogging);
        }

        private void CheckExistingProjectConfig(bool verboseLogging)
        {
            try
            {
                var existingProject = _projectConfigController.GetCurrentProjectConfig();
                if (existingProject != null && verboseLogging)
                {
                    Console.PrintWarning("An existing project configuration was found and will be replaced.");
                }
            }
            catch (Exception ex)
            {
                if (verboseLogging)
                {
                    Console.PrintDebug($"No existing project config found: {ex.Message}");
                }
                // This is expected for new projects
            }
        }

        private void CreateProjectConfig()
        {
            try
            {
                _projectConfigController.CreateProjectConfigFile();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create project configuration: {ex.Message}", ex);
            }
        }
    }
}
