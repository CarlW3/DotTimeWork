using DotTimeWork.ConsoleService;
using DotTimeWork.Developer;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class DeveloperCommand:Command
    {
        private readonly IDeveloperConfigController _developerConfigController;
        private readonly IInputAndOutputService _inputAndOutputService;
        public DeveloperCommand(IDeveloperConfigController developerConfigController, IInputAndOutputService inputAndOutputService) : base("Developer", "Create the Developer Profile")
        {
            _developerConfigController = developerConfigController;
            _inputAndOutputService = inputAndOutputService;
            this.SetHandler(Execute, PublicOptions.VerboseLogging);
            Description = "Creates Developer Config file. This will create a new developer config file in the current directory. The config file will be used to store the developer settings.";

        }

        internal void Execute(bool verboseLogging)
        {
            if (_developerConfigController.IsDeveloperConfigFileExisting() && verboseLogging)
            {
                _inputAndOutputService.PrintWarning("Developer config file already exists. Overwrite it...");
            }
            _developerConfigController.CreateDeveloperConfigFile();
            if (verboseLogging)
            {
                _inputAndOutputService.PrintDebug("Developer config file created successfully.");
            }
        }
    }
}
