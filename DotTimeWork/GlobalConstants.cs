using System.Reflection;

namespace DotTimeWork
{
    public static class GlobalConstants
    {
        public const string RELEASE_VERSION = "1.1";

        private const string ProjectConfigFilePath = "dottimework.json";
        private const string DeveloperConfigFilePath = "developer.json";

        public const string REPORT_HTML_TITLE = "Dot Time Worker Report";

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().Location;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Global location -> used for all projects the same Developer Config
        /// </summary>
        /// <returns></returns>
        public static string GetPathToDeveloperConfigFile()
        {
            return Path.Combine(AssemblyDirectory, DeveloperConfigFilePath);
        }


        /// <summary>
        /// Local location -> used for the current project
        /// </summary>
        /// <returns></returns>
        public static string GetPathToProjectConfigFile()
        {
            return Path.Combine(Environment.CurrentDirectory, ProjectConfigFilePath);
        }
    }
}
