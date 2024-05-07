using CommandLine;
using QnapBackupDecryptor.Core;
using QnapBackupDecryptor.Core.Models;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace QnapBackupDecryptor.Console;

class Program
{
    static void Main(string[] args)
    {
        Parser.Default
            .ParseArguments<Options>(args)
            .WithParsed(Run);
    }

    private static void Run(Options options)
    {
        // Double check delete is wanted
        if (Prompts.EnsureDeleteWanted(options) == false)
            return;

        // Double check in-place change is wanted
        if (Prompts.EnsureInPlaceWanted(options) == false)
            return;

        var password = Prompts.GetPassword(options);

        var stopwatch = Stopwatch.StartNew();

        var decryptJobs = GetDecryptJobs(options);

        var (decryptResults, deleteResults) = DoDecrypt(decryptJobs, options, password);

        stopwatch.Stop();

        Output.ShowResults(decryptResults, deleteResults, options.Verbose, stopwatch.Elapsed);
    }

    private static IReadOnlyList<FileJob> GetDecryptJobs(Options options)
    {
        return AnsiConsole
            .Status()
            .Start("Getting Files...", statusContext =>
            {
                statusContext.Spinner(Spinner.Known.SimpleDots);
                statusContext.SpinnerStyle(Style.Parse("green"));

                return JobMaker.GetDecryptJobs(
                        encryptedSource: options.EncryptedSource,
                        decryptedTarget: options.OutputDestination,
                        overwrite: options.Overwrite,
                        includeSubFolders: options.IncludeSubfolders);
            });
    }

    private static (IReadOnlyList<DecryptResult> DecryptResults, IReadOnlyList<DeleteResult> DeleteResults)
        DoDecrypt(IReadOnlyCollection<FileJob> decryptJobs, Options options, byte[] password)
    {
        var decryptResults = new ConcurrentBag<DecryptResult>();
        var deleteResults = new ConcurrentBag<DeleteResult>();

        AnsiConsole.Progress()
            .Columns(Output.GetProgressColumns())
            .AutoClear(true)
            .Start(progressContext =>
            {
                var progressTask = progressContext.AddTask("[green]Decrypting Files[/]");
                progressTask.MaxValue = decryptJobs.Count;

                Parallel.ForEach(decryptJobs, job =>
                    {
                        var (decryptResult, deleteResult) = DecryptorService.Decrypt(options.RemoveEncrypted, password, job, progressTask.Increment);
                        decryptResults.Add(decryptResult);
                        if (deleteResult != null)
                            deleteResults.Add(deleteResult);
                    });

            });

        return (decryptResults.ToList(), deleteResults.ToList());

    }

}