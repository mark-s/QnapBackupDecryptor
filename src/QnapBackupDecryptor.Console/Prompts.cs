using Spectre.Console;
using System.Text;

namespace QnapBackupDecryptor.Console
{
    internal class Prompts
    {
        public static byte[] GetPassword(Options opts)
        {
            if (string.IsNullOrEmpty(opts.Password) == false)
                return Encoding.UTF8.GetBytes(opts.Password);
            else
            {
                var password = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold]>> Enter password[/]")
                        .PromptStyle("yellow")
                        .Secret());
                return Encoding.UTF8.GetBytes(password);
            }
        }
    }
}
