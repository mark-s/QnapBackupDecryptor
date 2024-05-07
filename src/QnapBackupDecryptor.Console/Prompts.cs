using Spectre.Console;
using System.Text;

namespace QnapBackupDecryptor.Console;

internal static class Prompts
{
    private const string YES = "y";
    private const string NO = "n";
    private const string INVALID_OPTION_ENTERED = "[yellow]That's not a valid option[/]";

    public static byte[] GetPassword(Options options)
    {
        if (string.IsNullOrEmpty(options.Password) == false)
            return Encoding.UTF8.GetBytes(options.Password);
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
                .WithYesNoOptions(defaultOption: NO));

        return response.IsYes();
    }

    public static bool EnsureInPlaceWanted(Options options)
    {
        if (options.InPlace == false)
            return true;

        var initialResponse = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]>> Are you sure you want to decrypt the files in-place? If a decrypt produces a bad file - you will lose that file![/]")
                .WithYesNoOptions(defaultOption: NO));

        var areYouSureResponse = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]>> Are you really sure? Do you have a backup in case anything goes wrong?[/]")
                .WithYesNoOptions(defaultOption: NO));

        return initialResponse.IsYes() && areYouSureResponse.IsYes();
    }

    private static TextPrompt<string> WithYesNoOptions(this TextPrompt<string> prompt, string defaultOption)
        => prompt.InvalidChoiceMessage(INVALID_OPTION_ENTERED)
            .DefaultValue(defaultOption)
            .AddChoice(YES)
            .AddChoice(NO);

    private static bool IsYes(this string? value)
        => value?.Equals(YES, StringComparison.OrdinalIgnoreCase) ?? false;

}