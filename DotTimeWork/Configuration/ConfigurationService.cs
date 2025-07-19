using DotTimeWork.Common;
using System.Text.Json;

namespace DotTimeWork.Configuration
{
    /// <summary>
    /// Configuration settings for the application
    /// </summary>
    public class AppConfiguration
    {
        public string ProjectConfigFileName { get; set; } = "dottimework.json";
        public string DeveloperConfigFileName { get; set; } = "developer.json";
        public string DefaultTimeTrackingFolder { get; set; } = "TimeTracking";
        public bool VerboseLogging { get; set; } = false;
        public int DefaultHoursPerDay { get; set; } = 8;
        public string ReportTitle { get; set; } = "Dot Time Worker Report";
    }

    /// <summary>
    /// Service for managing application configuration
    /// </summary>
    public interface IConfigurationService
    {
        AppConfiguration Configuration { get; }
        Result UpdateConfiguration(Action<AppConfiguration> updateAction);
        Result SaveConfiguration();
        Result LoadConfiguration();
        string GetProjectConfigPath();
        string GetDeveloperConfigPath();
    }

    internal class ConfigurationService : IConfigurationService
    {
        private const string ConfigFileName = "dottimework.config.json";
        private AppConfiguration _configuration = new();

        public AppConfiguration Configuration => _configuration;

        public Result UpdateConfiguration(Action<AppConfiguration> updateAction)
        {
            try
            {
                updateAction(_configuration);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        public Result SaveConfiguration()
        {
            try
            {
                var configPath = Path.Combine(GetApplicationDirectory(), ConfigFileName);
                var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(configPath, json);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        public Result LoadConfiguration()
        {
            try
            {
                var configPath = Path.Combine(GetApplicationDirectory(), ConfigFileName);
                if (!File.Exists(configPath))
                {
                    // Use defaults if no config file exists
                    return Result.Success();
                }

                var json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json);
                if (config != null)
                {
                    _configuration = config;
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        public string GetProjectConfigPath()
        {
            return Path.Combine(Environment.CurrentDirectory, _configuration.ProjectConfigFileName);
        }

        public string GetDeveloperConfigPath()
        {
            return Path.Combine(GetApplicationDirectory(), _configuration.DeveloperConfigFileName);
        }

        private static string GetApplicationDirectory()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;
        }
    }
}
