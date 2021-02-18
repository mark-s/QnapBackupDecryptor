using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QnapBackupDecryptor.Console
{
    internal class Output
    {
        public static void ShowResults(ConcurrentBag<DecryptResult> results, bool verbose, TimeSpan swElapsed)
        {
            if (verbose)
                ShowFileListResults(results);
            else
                ShowSimpleResults(results);

            ShowTiming(swElapsed);
        }

        private static void ShowSimpleResults(ConcurrentBag<DecryptResult> results)
        {
            AnsiConsole.MarkupLine($"[bold]Total encrypted files: {results.Count}[/]");
            AnsiConsole.MarkupLine($"[green]Decrypted\t{results.Count(r => r.Success)}[/]");
            AnsiConsole.MarkupLine($"[red]Failed\t\t{ results.Count(r => !r.Success)}[/]");
        }

        private static void ShowFileListResults(ConcurrentBag<DecryptResult> results)
        {
            var table = new Table();
            table.AddColumn("Status");
            table.AddColumn(new TableColumn("Encrypted").Centered());
            table.AddColumn(new TableColumn("Decrypted").Centered());
            if (results.Any(r => r.Success == false))
            {
                table.AddColumn("Error Message");
                Environment.ExitCode = 1;
            }

            foreach (var result in results)
                table.AddRow(ResultToText(result).ToArray());

            AnsiConsole.Render(table);

            AnsiConsole.MarkupLine($"[bold]Total {results.Count} - Decrypted {results.Count(r => r.Success)}[/]");
        }

        private static IEnumerable<string> ResultToText(DecryptResult result)
        {
            var colour = result.Success ? "green" : "red";

            if (result.Success)
                return new List<string>
                {
                    $"[{colour}]Ok[/]",
                    $"[{colour}]{result.Source.FullName}[/]",
                    $"[{colour}]{result.Dest.FullName}[/]"
                };
            else
                return new List<string>
                {
                    $"[{colour}]Ok[/]",
                    $"[{colour}]{result.Source.FullName}[/]",
                    $"[{colour}]{result.Dest.FullName}[/]",
                    $"[{colour}]{result.ErrorMessage}[/]"
                };

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

    }
}
