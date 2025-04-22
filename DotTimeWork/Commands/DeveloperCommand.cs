using DotTimeWork.Developer;
using System.CommandLine;

namespace DotTimeWork.Commands
{
    internal class DeveloperCommand:Command
    {
        public DeveloperCommand(IDeveloperConfigController developerConfigController) : base("Developer", "Create the Developer Profile")
        {
            this.SetHandler(developerConfigController.CreateDeveloperConfigFile);
            Description = "Creates Developer Config file. This will create a new developer config file in the current directory. The config file will be used to store the developer settings.";
        }
    }
}
