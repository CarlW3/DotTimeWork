using Spectre.Console;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Runtime.ConstrainedExecution;

namespace DotTimeWork.Developer
{
    /// <summary>
    /// This should by DI and singleton
    /// </summary>
    public class DeveloperConfigController : IDeveloperConfigController
    {
        private DeveloperConfig _currentDeveloperConfig;

        public void CreateDeveloperConfigFile()
        {
            if (File.Exists(GlobalConstants.GetPathToDeveloperConfigFile()))
            {
                Console.WriteLine("Developer config file already exists. Deleting it.");
                File.Delete(GlobalConstants.GetPathToDeveloperConfigFile());
            }
            Console.WriteLine("Creating developer config file...");


            var developerName = AnsiConsole.Ask<string>("Developer Name:");

            var developerEmail = AnsiConsole.Ask<string>("Please enter the developer email:", string.Empty);
            if (string.IsNullOrEmpty(developerEmail))
            {
                Console.WriteLine("Developer email cannot be empty. Using default value of 'N/A'.");
                developerEmail = "N/A";
            }

            var hoursPerDayWork = AnsiConsole.Prompt(
            new TextPrompt<int>("Please enter the hours per day work(default is 8):")
                    .DefaultValue(8)
                    .Validate((n) => n switch
                     {
                         < 0 => ValidationResult.Error("Negative time is not allowed"),
                         > 24 => ValidationResult.Error("More than 24 hours not possible"),
                         _ => ValidationResult.Success(),
                     }));


            _currentDeveloperConfig = new DeveloperConfig
            {
                Name = developerName,
                E_Mail = developerEmail,
                HoursPerDayWork = hoursPerDayWork
            };

            var json = System.Text.Json.JsonSerializer.Serialize(_currentDeveloperConfig);
            File.WriteAllText(GlobalConstants.GetPathToDeveloperConfigFile(), json);
            Console.WriteLine("Developer config file created.");
        }

        public void AssignTaskToCurrentDeveloper(string name)
        {

            CurrentDeveloperConfig.AddStartedTask(name);

            PersistChangesInDeveloperConfig();
        }

        private void PersistChangesInDeveloperConfig()
        {
            string targetPath = GlobalConstants.GetPathToDeveloperConfigFile();
            if (File.Exists(targetPath))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(_currentDeveloperConfig);
                File.WriteAllText(GlobalConstants.GetPathToDeveloperConfigFile(), json);
            }
            else
            {
                Console.WriteLine("Developer config file not found. Cannot persist changes => Please create new Developer Config");
            }
        }

        private DeveloperConfig? LoadDeveloperConfig()
        {
            if (!File.Exists(GlobalConstants.GetPathToDeveloperConfigFile()))
            {
                Console.WriteLine("Developer config file not found.");
                return null;
            }

            var json = File.ReadAllText(GlobalConstants.GetPathToDeveloperConfigFile());
            DeveloperConfig? developerConfig = System.Text.Json.JsonSerializer.Deserialize<DeveloperConfig>(json);
            if (developerConfig == null)
            {
                Console.WriteLine("Failed to load developer config.");
                return null;
            }
            return developerConfig;
        }

        public DeveloperConfig CurrentDeveloperConfig
        {
            get
            {
                if (_currentDeveloperConfig == null)
                {
                    _currentDeveloperConfig = LoadDeveloperConfig() ?? throw new InvalidOperationException("Could not load Developer Configuration");
                }
                return _currentDeveloperConfig;
            }
        }
    }
}
