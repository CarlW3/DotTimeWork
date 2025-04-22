using DotTimeWork.Commands;
using Spectre.Console;
using System;
using System.IO;

namespace DotTimeWork.Project
{
    /// <summary>
    /// This should by DI and singlton
    /// </summary>
    public class ProjectConfigController : IProjectConfigController
    {
        private ProjectConfig _currentProjectConfig;

        public bool LoadProjectConfig()
        {
            if (!File.Exists(GlobalConstants.GetPathToProjectConfigFile()))
            {
                Console.WriteLine("Project config file not found.");
                return false;
            }

            var json = File.ReadAllText(GlobalConstants.GetPathToProjectConfigFile());
            _currentProjectConfig = System.Text.Json.JsonSerializer.Deserialize<ProjectConfig>(json);
            if (_currentProjectConfig == null)
            {
                Console.WriteLine("Failed to load project config.");
                return false;
            }
            return true;
        }

        public void CreateProjectConfigFile()
        {
            if (File.Exists(GlobalConstants.GetPathToProjectConfigFile()))
            {
                Console.WriteLine("Project config file already exists. Deleting it.");
                File.Delete(GlobalConstants.GetPathToProjectConfigFile());
            }
            Console.WriteLine("Creating project config file...");

            var projectName = AnsiConsole.Ask<string>("Please enter the project name:");
            if (string.IsNullOrEmpty(projectName))
            {
                Console.WriteLine("Project name cannot be empty. Using default value of 'My Project'.");
                projectName = "My Project";
            }

            Console.WriteLine("Please enter the project start date (yyyy-MM-dd or EMPTY for today):");
            var projectStartDate = Console.ReadLine();
            Console.WriteLine("Please enter the project end date (yyyy-MM-dd or EMPTY for N/A):");
            var projectEndDate = Console.ReadLine();

            Console.WriteLine("Please enter the max time per day (in hours):");
            var maxTimePerDay = Console.ReadLine();

            DateTime projectStart;

            if (string.IsNullOrWhiteSpace(projectStartDate))
            {
                projectStart = DateTime.Now;
            }
            else if (!DateTime.TryParse(projectStartDate, out projectStart))
            {
                Console.WriteLine("Invalid project start date. Using current date.");
                projectStart = DateTime.Now;
            }
            if (!int.TryParse(maxTimePerDay, out int maxTime))
            {
                Console.WriteLine("Invalid max time per day. Using default value of 8 hours.");
                maxTime = 8;
            }

            DateTime projectEnd;
            if(string.IsNullOrEmpty(projectEndDate))
            {
                projectEnd = DateTime.MinValue;
            }
            else if (!DateTime.TryParse(projectEndDate, out projectEnd))
            {
                Console.WriteLine("Invalid project end date. Using current date + 30 days.");
                projectEnd = DateTime.Now.AddDays(30);
            }

            _currentProjectConfig = new ProjectConfig
            {
                ProjectName = projectName,
                MaxTimePerDay = maxTime,
                ProjectStart = projectStart,
                ProjectEnd = projectEnd == DateTime.MinValue ? null : projectEnd,
            };

            var json = System.Text.Json.JsonSerializer.Serialize(_currentProjectConfig);
            File.WriteAllText(GlobalConstants.GetPathToProjectConfigFile(), json);
            Console.WriteLine("Project config file created.");
            string timeTrackingFolder = Path.Combine(Environment.CurrentDirectory, _currentProjectConfig.TimeTrackingFolder);
            Directory.CreateDirectory(timeTrackingFolder);
            Console.WriteLine($"Time tracking folder created at {timeTrackingFolder}");
            File.WriteAllText(Path.Combine(timeTrackingFolder, "README.txt"), "This folder contains the time tracking files for the project." + Environment.NewLine + "Created: " + DateTime.Now);
            Console.WriteLine($"README.txt file created in {timeTrackingFolder}");
        }

        public ProjectConfig GetCurrentProjectConfig()
        {
            if (_currentProjectConfig == null)
            {
                if (PublicOptions.IsVerbosLogging)
                {
                    Console.WriteLine("Project config file not loaded. Loading now...");
                }
                LoadProjectConfig();
            }
            return _currentProjectConfig;
        }
    }
}
