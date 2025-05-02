using DotTimeWork.Developer;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class DeveloperCommand:Command
    {
        private readonly IDeveloperConfigController _developerConfigController;
        public DeveloperCommand(IDeveloperConfigController developerConfigController) : base("Developer", "Create the Developer Profile")
        {
            _developerConfigController = developerConfigController;
            this.SetHandler(Execute,PublicOptions.VerboseLogging);
            Description = "Creates Developer Config file. This will create a new developer config file in the current directory. The config file will be used to store the developer settings.";
        }

        private void Execute(bool verboseLogging)
        {
            if (_developerConfigController.IsDeveloperConfigFileExisting() && verboseLogging)
            {
                AnsiConsole.MarkupLine($"[red]Developer config file already exists![/]");
                AnsiConsole.MarkupLine($"[red]New File will be generated[/]");
            }
            _developerConfigController.CreateDeveloperConfigFile();
            if (verboseLogging)
            {
                AnsiConsole.MarkupLine($"[grey]Developer configurion generation finished[/]");
            }
        }
    }
}
