using DotTimeWork.ConsoleService;
using DotTimeWork.Infrastructure;
using System.CommandLine;

namespace DotTimeWork.Commands.Base
{
    /// <summary>
    /// Base class for all commands providing common functionality
    /// </summary>
    internal abstract class BaseCommand : Command
    {
        protected readonly IInputAndOutputService Console;

        protected BaseCommand(string name, string description) : base(name, description)
        {
            Console = GetInputAndOutputService();
            SetupCommand();
        }

        protected BaseCommand(string name, string description, IInputAndOutputService inputAndOutputService) : base(name, description)
        {
            Console = inputAndOutputService ?? throw new ArgumentNullException(nameof(inputAndOutputService));
            SetupCommand();
        }

        private static IInputAndOutputService GetInputAndOutputService()
        {
            try
            {
                return ServiceContainer.GetService<IInputAndOutputService>();
            }
            catch (InvalidOperationException)
            {
                // ServiceContainer not initialized (likely in tests), return a minimal implementation
                return new TestConsoleService();
            }
        }

        /// <summary>
        /// Minimal implementation for testing scenarios when ServiceContainer is not available
        /// </summary>
        private class TestConsoleService : IInputAndOutputService
        {
            public void PrintError(string text) { }
            public void PrintInfo(string text) { }
            public void PrintNormal(string text) { }
            public void PrintSuccess(string text) { }
            public void PrintWarning(string text) { }
            public void PrintDebug(string text) { }
            public void PrintMarkup(string text) { }
            public T AskForInput<T>(string text) => default(T)!;
            public string AskForInput(string text, string defaultText) => defaultText;
            public int AskForInput(string text, int defaultValue) => defaultValue;
            public string ShowTaskSelection(string[] availableTasks, string promptText) => availableTasks.FirstOrDefault() ?? "";
        }

        /// <summary>
        /// Override this method to set up command-specific options and handlers
        /// </summary>
        protected abstract void SetupCommand();

        /// <summary>
        /// Executes the command with error handling and verbose logging
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="verboseLogging">Enable verbose logging</param>
        protected void ExecuteWithErrorHandling(Action action, bool verboseLogging = false)
        {
            try
            {
                PublicOptions.IsVerbosLogging = verboseLogging;
                
                if (verboseLogging)
                {
                    Console.PrintDebug($"Executing command: {Name}");
                }

                action();

                if (verboseLogging)
                {
                    Console.PrintDebug($"Command {Name} completed successfully");
                }
            }
            catch (Exception ex)
            {
                Console.PrintError($"Error executing command '{Name}': {ex.Message}");
                if (verboseLogging)
                {
                    Console.PrintDebug($"Stack trace: {ex.StackTrace}");
                }
                throw;
            }
        }

        /// <summary>
        /// Helper method to validate required parameters
        /// </summary>
        protected void ValidateRequiredParameter(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Parameter '{parameterName}' is required but was not provided.");
            }
        }

        /// <summary>
        /// Helper method to get a parameter value or prompt the user for it
        /// </summary>
        protected string GetOrPromptForValue(string? providedValue, string promptText, string? defaultValue = null)
        {
            if (!string.IsNullOrWhiteSpace(providedValue))
            {
                return providedValue;
            }

            return string.IsNullOrEmpty(defaultValue) 
                ? Console.AskForInput<string>(promptText)
                : Console.AskForInput(promptText, defaultValue);
        }
    }
}
