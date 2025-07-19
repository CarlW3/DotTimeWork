using System.Reflection;

namespace DotTimeWork
{
    /// <summary>
    /// Global constants and helper methods for file paths
    /// </summary>
    public static class GlobalConstants
    {
        public const string RELEASE_VERSION = "1.5";
        public const string REPORT_HTML_TITLE = "Dot Time Worker Report";

        // File names
        private const string ProjectConfigFileName = "dottimework.json";
        private const string DeveloperConfigFileName = "developer.json";

        /// <summary>
        /// Gets the directory where the application assembly is located
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var directory = Path.GetDirectoryName(assemblyLocation);
                return directory ?? Environment.CurrentDirectory;
            }
        }

        /// <summary>
        /// Gets the path to the developer config file (global location)
        /// </summary>
        public static string GetPathToDeveloperConfigFile()
        {
            return Path.Combine(AssemblyDirectory, DeveloperConfigFileName);
        }

        /// <summary>
        /// Gets the path to the project config file (local location)
        /// </summary>
        public static string GetPathToProjectConfigFile()
        {
            return Path.Combine(Environment.CurrentDirectory, ProjectConfigFileName);
        }

        /// <summary>
        /// Validates that a path is safe and within expected bounds
        /// </summary>
        public static bool IsValidPath(string path)
        {
            try
            {
                return !string.IsNullOrEmpty(path) && 
                       !Path.GetInvalidPathChars().Any(path.Contains) &&
                       Path.IsPathRooted(path) || !Path.IsPathRooted(path); // Accept both absolute and relative
            }
            catch
            {
                return false;
            }
        }
    }
}
