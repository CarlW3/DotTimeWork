namespace DotTimeWork.ConsoleService
{
    public interface IInputAndOutputService
    {
        T AskForInput<T>(string text);

        string AskForInput(string text, string defaultText);
        int AskForInput(string text, int defaultValue);
        void PrintError(string text);
        void PrintInfo(string text);
        void PrintNormal(string text);
        void PrintSuccess(string text);
        void PrintWarning(string text);
        void PrintMarkup(string text);
        void PrintDebug(string text);
        string ShowTaskSelection(string[] availableTasks, string promptText);
    }
}