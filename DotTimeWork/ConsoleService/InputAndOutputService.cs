using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTimeWork.ConsoleService
{
    internal class InputAndOutputService : IInputAndOutputService
    {
        public void PrintNormal(string text)
        {
            AnsiConsole.WriteLine(text);
        }

        public void PrintMarkup(string text)
        {
            AnsiConsole.MarkupLine(text);
        }

        public void PrintError(string text)
        {
            AnsiConsole.MarkupLine($"[red]{text}[/]");
        }
        public void PrintSuccess(string text)
        {
            AnsiConsole.MarkupLine($"[green]{text}[/]");
        }
        public void PrintWarning(string text)
        {
            AnsiConsole.MarkupLine($"[yellow]{text}[/]");
        }
        public void PrintInfo(string text)
        {
            AnsiConsole.MarkupLine($"[blue]{text}[/]");
        }

        public void PrintDebug(string text)
        {
            AnsiConsole.MarkupLine($"[grey]{text}[/]");
        }

        public string ShowTaskSelection(string[] availableTasks, string promptText)
        {

            if (availableTasks == null || availableTasks.Length == 0)
            {
                AnsiConsole.MarkupLine($"[red]No tasks found. Please create a task first.[/]");
                return string.Empty;
            }
            if (availableTasks.Length == 1)
            {
                string toReturn = availableTasks[0];
                AnsiConsole.MarkupLine($"[green]Only one task found. Using '{toReturn}' as task.[/]");
                return toReturn;
            }
            return AnsiConsole.Prompt(
                 new SelectionPrompt<string>()
                     .Title(promptText)
                     .PageSize(5)
                     .AddChoices(availableTasks));
        }

        public string AskForInput(string text, string defaultText)
        {
            return AnsiConsole.Ask<string>(text, defaultText);
        }

        public int AskForInput(string text, int defaultValue)
        {
            return AnsiConsole.Ask<int>(text,defaultValue);
        }

        public T AskForInput<T>(string text)
        {
            return AnsiConsole.Ask<T>(text);
        }
    }
}
