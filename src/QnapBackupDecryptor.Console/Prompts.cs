using Spectre.Console;
using System.Text;

namespace QnapBackupDecryptor.Console;

internal static class Prompts
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

    public static bool EnsureDeleteWanted(Options options)
    {
        if (options.RemoveEncrypted == false)
            return true;

        var response = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]>> Are you sure you want to delete the encrypted files?[/]")
                .InvalidChoiceMessage("[yellow]That's not a valid option[/]")
                .DefaultValue("n")
                .AddChoice("y")
                .AddChoice("n"));

        return response.Equals("y", StringComparison.InvariantCultureIgnoreCase);
    }
}