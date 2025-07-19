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
            Console = ServiceContainer.GetService<IInputAndOutputService>();
            SetupCommand();
        }

        /// <summary>
        /// Override this method to set up command-specific options and handlers
        /// </summary>
        protected abstract void SetupCommand();

        /// <summary>
        /// Executes the command with error handling and verbose logging
        /// </summary>
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
        /// Executes the command with error handling and returns a result
        /// </summary>
        protected T ExecuteWithErrorHandling<T>(Func<T> func, bool verboseLogging = false)
        {
            try
            {
                PublicOptions.IsVerbosLogging = verboseLogging;
                
                if (verboseLogging)
                {
                    Console.PrintDebug($"Executing command: {Name}");
                }

                var result = func();

                if (verboseLogging)
                {
                    Console.PrintDebug($"Command {Name} completed successfully");
                }

                return result;
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
