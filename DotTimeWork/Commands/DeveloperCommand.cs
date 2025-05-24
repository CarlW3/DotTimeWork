using DotTimeWork.ConsoleService;
using DotTimeWork.Developer;
using Spectre.Console;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class DeveloperCommand : Command
    {
        private readonly IDeveloperConfigController _developerConfigController;
        private readonly IInputAndOutputService _inputAndOutputService;
        public DeveloperCommand(IDeveloperConfigController developerConfigController, IInputAndOutputService inputAndOutputService) : base("Developer", "Create the Developer Profile")
        {
            _developerConfigController = developerConfigController;
            _inputAndOutputService = inputAndOutputService;
            this.SetHandler(Execute, PublicOptions.VerboseLogging);
            Description = Properties.Resources.Developer_Description;

        }

        internal void Execute(bool verboseLogging)
        {
            if (_developerConfigController.IsDeveloperConfigFileExisting() && verboseLogging)
            {
                _inputAndOutputService.PrintWarning(Properties.Resources.Developer_Create_ExistsAlready);
            }
            _developerConfigController.CreateDeveloperConfigFile();

            _inputAndOutputService.PrintSuccess(Properties.Resources.Developer_Create_Success);
        }
    }
}
