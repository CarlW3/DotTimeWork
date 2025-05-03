using DotTimeWork.Project;

namespace DotTimeWork.DataProvider
{
    public interface IProjectConfigDataProvider
    {
        void DeleteProjectConfigFile();
        ProjectConfig LoadProjectConfig();
        void PersistProjectConfig(ProjectConfig toPersist);
        bool ProjectConfigFileExists();
    }
}