using DotTimeWork.Commands.Base;
using DotTimeWork.Developer;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class DeveloperCommand : BaseCommand
    {
        private readonly IDeveloperConfigController _developerConfigController;

        public DeveloperCommand(IDeveloperConfigController developerConfigController) 
            : base("Developer", Properties.Resources.Developer_Description)
        {
            _developerConfigController = developerConfigController ?? throw new ArgumentNullException(nameof(developerConfigController));
        }

        protected override void SetupCommand()
        {
            this.SetHandler(Execute, PublicOptions.VerboseLogging);
            
            // Add sub-commands for better functionality
            var showCommand = new Command("show", "Show current developer configuration");
            showCommand.SetHandler(ShowDeveloperConfig, PublicOptions.VerboseLogging);
            AddCommand(showCommand);
        }

        private void Execute(bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                CheckExistingDeveloperConfig(verboseLogging);
                CreateDeveloperConfig();
                Console.PrintSuccess(Properties.Resources.Developer_Create_Success);
            }, verboseLogging);
        }

        private void ShowDeveloperConfig(bool verboseLogging)
        {
            ExecuteWithErrorHandling(() =>
            {
                if (!_developerConfigController.IsDeveloperConfigFileExisting())
                {
                    Console.PrintWarning("No developer configuration found. Please create one first using 'dottimework developer'.");
                    return;
                }

                try
                {
                    var config = _developerConfigController.CurrentDeveloperConfig;
                    if (config != null)
                    {
                        DisplayDeveloperConfig(config, verboseLogging);
                    }
                    else
                    {
                        Console.PrintError("Failed to load developer configuration.");
                    }
                }
                catch (Exception ex)
                {
                    Console.PrintError($"Error loading developer configuration: {ex.Message}");
                    if (verboseLogging)
                    {
                        Console.PrintDebug($"Stack trace: {ex.StackTrace}");
                    }
                }
            }, verboseLogging);
        }

        private void CheckExistingDeveloperConfig(bool verboseLogging)
        {
            if (_developerConfigController.IsDeveloperConfigFileExisting())
            {
                if (verboseLogging)
                {
                    Console.PrintWarning(Properties.Resources.Developer_Create_ExistsAlready);
                }
                else
                {
                    Console.PrintInfo("Existing developer configuration will be updated.");
                }
            }
        }

        private void CreateDeveloperConfig()
        {
            try
            {
                _developerConfigController.CreateDeveloperConfigFile();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create developer configuration: {ex.Message}", ex);
            }
        }

        private void DisplayDeveloperConfig(DeveloperConfig config, bool verboseLogging)
        {
            Console.PrintInfo("Current Developer Configuration:");
            Console.PrintMarkup($"[green]Name:[/] {config.Name}");
            Console.PrintMarkup($"[green]Email:[/] {config.E_Mail ?? "Not specified"}");
            Console.PrintMarkup($"[green]Hours per Day:[/] {config.HoursPerDayWork}");

            if (config.StartedTasks?.Any() == true)
            {
                Console.PrintMarkup($"[green]Active Tasks:[/] {string.Join(", ", config.StartedTasks)}");
            }
            else
            {
                Console.PrintMarkup("[yellow]No active tasks[/]");
            }

            if (verboseLogging)
            {
                var configPath = GlobalConstants.GetPathToDeveloperConfigFile();
                Console.PrintDebug($"Configuration file location: {configPath}");
            }
        }
    }
}
