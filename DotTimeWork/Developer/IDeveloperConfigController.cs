namespace DotTimeWork.Developer
{
    public interface IDeveloperConfigController
    {
        void AssignTaskToCurrentDeveloper(string name);
        void CreateDeveloperConfigFile();
        bool IsDeveloperConfigFileExisting();

        DeveloperConfig CurrentDeveloperConfig { get; }
    }
}
