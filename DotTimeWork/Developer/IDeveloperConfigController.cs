namespace DotTimeWork.Developer
{
    public interface IDeveloperConfigController
    {
        void AssignTaskToCurrentDeveloper(string name);
        void CreateDeveloperConfigFile();
        DeveloperConfig CurrentDeveloperConfig { get; }
    }
}
