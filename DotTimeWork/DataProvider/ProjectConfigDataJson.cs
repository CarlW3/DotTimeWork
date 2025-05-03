using DotTimeWork.Project;

namespace DotTimeWork.DataProvider
{
    internal class ProjectConfigDataJson : IProjectConfigDataProvider
    {
        private readonly string _pathToProjectConfigFile;

        public ProjectConfigDataJson():this(GlobalConstants.GetPathToProjectConfigFile())
        {
            // Default constructor for DI
        }

        public ProjectConfigDataJson(string pathToProjectConfigFile)
        {
            _pathToProjectConfigFile = pathToProjectConfigFile;
        }

        
        public bool ProjectConfigFileExists()
        {
            return File.Exists(_pathToProjectConfigFile);
        }

        public void DeleteProjectConfigFile()
        {
            if (File.Exists(_pathToProjectConfigFile))
            {
                File.Delete(_pathToProjectConfigFile);
            }
        }

        public ProjectConfig LoadProjectConfig()
        {
            if (!File.Exists(_pathToProjectConfigFile))
            {
                return null;
            }

            var json = File.ReadAllText(_pathToProjectConfigFile);
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("Project config file is empty.");
                return null;
            }
            ProjectConfig currentProjectConfig = System.Text.Json.JsonSerializer.Deserialize<ProjectConfig>(json);
            if (currentProjectConfig == null)
            {
                Console.WriteLine("Failed to load project config.");
                return null;
            }
            return currentProjectConfig;
        }

        public void PersistProjectConfig(ProjectConfig toPersist)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(toPersist);
            File.WriteAllText(_pathToProjectConfigFile, json);
        }
    }
}
