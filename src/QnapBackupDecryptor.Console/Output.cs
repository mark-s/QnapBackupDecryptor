﻿using QnapBackupDecryptor.Core.Models;
using Spectre.Console;

namespace QnapBackupDecryptor.Console;

internal static class Output
{
    public static void ShowResults(IReadOnlyList<DecryptResult> decryptResults, IReadOnlyList<DeleteResult> deleteResults, bool verbose, TimeSpan elapsedTime)
    {
        if (verbose && decryptResults.Count > 1)
        {
            ShowFileListResults(decryptResults, deleteResults);
            ShowSimpleResults(decryptResults, deleteResults);
        }
        else
        {
            ShowSimpleResults(decryptResults, deleteResults);
            AnsiConsole.MarkupLine("Add --verbose to see details.");
        }

        ShowTiming(elapsedTime);

        if (decryptResults.Any(r => r.DecryptedOk == false) || deleteResults.Any(r => r.DeletedOk == false))
            Environment.ExitCode = 1;
    }

    private static void ShowSimpleResults(IReadOnlyList<DecryptResult> decryptResults, IReadOnlyList<DeleteResult> deleteResults)
    {
        var table = new Table();
        table
            .AddColumn(new TableColumn("Total encrypted files").RightAligned())
            .AddColumn(new TableColumn("Decrypt Ok").RightAligned())
            .AddColumn(new TableColumn("Decrypt Failed").RightAligned())
            .AddColumn(new TableColumn("Delete Ok").RightAligned())
            .AddColumn(new TableColumn("Delete Failed").RightAligned());

        table.AddRow(
            $"{decryptResults.Count}",
            $"[green]{decryptResults.Count(r => r.DecryptedOk)}[/]",
            $"[red]{decryptResults.Count(r => !r.DecryptedOk)}[/]",
            $"[green]{deleteResults.Count(r => r.DeletedOk)}[/]",
            $"[red]{deleteResults.Count(r => !r.DeletedOk)}[/]"
        );

        AnsiConsole.Write(table);
    }

    private static void ShowFileListResults(IReadOnlyList<DecryptResult> decryptResults, IReadOnlyList<DeleteResult> deleteResults)
    {
        var table = new Table();
        table
            .AddColumn(new TableColumn("Decrypt Status").Centered())
            .AddColumn("Encrypted")
            .AddColumn("Decrypted");

        if (decryptResults.Any(r => r.DecryptedOk == false))
            table.AddColumn("Error");

        if (deleteResults.Any())
        {
            table.AddColumn(new TableColumn("Delete Status").Centered());
            if (deleteResults.Any(r => r.DeletedOk == false))
                table.AddColumn("Error");
        }

        var deleteResultsLookup = deleteResults.ToDictionary(r => r.FileToDelete, r => r);

        foreach (var result in decryptResults)
        {
            var resultRow = DecryptResultToRow(result);

            if (deleteResultsLookup.TryGetValue(result.Source, out var deleteResult))
                resultRow.AddRange(DeleteResultToRow(deleteResult));

            table.AddRow(resultRow.ToArray());
        }

        AnsiConsole.Write(table);

    }

    private static List<string> DecryptResultToRow(DecryptResult decryptResult)
    {
        var colour = decryptResult.DecryptedOk ? "green" : "red";
        var status = decryptResult.DecryptedOk ? "OK" : "Fail";

        var row = new List<string>
        {
            $"[{colour}]{status}[/]",
            $"[{colour}]{decryptResult.Source.FullName}[/]",
            $"[{colour}]{decryptResult.Dest.FullName}[/]"
        };

        if (decryptResult.DecryptedOk == false)
            row.Add($"[{colour}]{decryptResult.ErrorMessage}[/]");

        return row;
    }

    private static List<string> DeleteResultToRow(DeleteResult? deleteResult)
    {
        if (deleteResult == null)
            return [];

        var colour = deleteResult.DeletedOk ? "green" : "red";
        var status = deleteResult.DeletedOk ? "Deleted" : "Failed";

        var row = new List<string>
        {
            $"[{colour}]{status}[/]"
        };

        if (deleteResult.DeletedOk == false)
            row.Add($"[{colour}]{deleteResult.ErrorMessage}[/]");

        return row;
    }

    private static void ShowTiming(TimeSpan swElapsed)
    {
        switch (swElapsed.TotalMinutes)
        {
            case < 1:
                AnsiConsole.MarkupLine($"[bold]Took {swElapsed.TotalSeconds:0.000} seconds[/]");
                break;
            default:
                AnsiConsole.MarkupLine($"[bold]Took {swElapsed.TotalMinutes} minutes[/]");
                break;
        }
    }

    public static ProgressColumn[] GetProgressColumns()
        => [
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new SpinnerColumn(Spinner.Known.SimpleDots)
        ];
}