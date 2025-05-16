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

        public string AskForInput(string text, string defaultText)
        {
            return AnsiConsole.Ask<string>(text, defaultText);
        }

        public int AskForInput(string text, int defaultValue)
        {
            return AnsiConsole.Ask<int>(text,defaultValue);
        }

        public string AskForStringInput(string text)
        {
            return AnsiConsole.Ask<string>(text);
        }
    }
}
