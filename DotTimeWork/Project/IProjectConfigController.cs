namespace DotTimeWork.Project
{
    public interface IProjectConfigController
    {
        void CreateProjectConfigFile();
        ProjectConfig GetCurrentProjectConfig();
    }
}