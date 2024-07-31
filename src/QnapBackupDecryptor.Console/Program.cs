using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using CommandLine;
using QnapBackupDecryptor.Core;
using QnapBackupDecryptor.Core.Models;
using Spectre.Console;

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

        var stopwatch = Stopwatch.StartNew();

        // Decrypt Files
        var decryptJobs = GetDecryptJobs(options).Where(j => j.IsValid).ToList();
        var decryptResults = Decrypt(decryptJobs, GetPassword(options));

        // Delete Files (if requested) 
        var filesToDelete = GetFilesToDelete(decryptResults, options);
        var deleteResults = Delete(filesToDelete);

        stopwatch.Stop();

        Output.ShowResults(decryptResults, deleteResults, options.Verbose, stopwatch.Elapsed);
    }

    private static byte[] GetPassword(Options options)
        => string.IsNullOrEmpty(options.Password)
            ? Prompts.GetPassword()
            : Encoding.UTF8.GetBytes(options.Password);

    private static IReadOnlyList<DecryptJob> GetDecryptJobs(Options options)
    {
        return AnsiConsole
            .Status()
            .Start("Getting Files...", statusContext =>
            {
                statusContext.Spinner(Spinner.Known.SimpleDots);
                statusContext.SpinnerStyle(Style.Parse("green"));

                return JobMaker.CreateDecryptJobs(
                    encryptedSource: options.EncryptedSource,
                    decryptedTarget: options.OutputDestination,
                    overwrite: options.Overwrite,
                    includeSubFolders: options.IncludeSubfolders);
            });
    }

    private static IReadOnlyList<DecryptResult> Decrypt(IReadOnlyCollection<DecryptJob> decryptJobs, byte[] password)
    {
        var decryptResults = new ConcurrentBag<DecryptResult>();

        AnsiConsole.Progress()
            .Columns(Output.GetProgressColumns())
            .AutoClear(true)
            .Start(progressContext =>
            {
                var progressTask = progressContext.AddTask("[green]Decrypting Files[/]");
                progressTask.MaxValue = decryptJobs.Count;

                Parallel.ForEach(decryptJobs, job =>
                {
                    var decryptResult = DecryptorService.TryDecrypt(password, job);
                    decryptResults.Add(decryptResult);
                    progressTask.Increment(1);
                });
            });

        return decryptResults.ToList();
    }

    private static IReadOnlyList<FileSystemInfo> GetFilesToDelete(IReadOnlyList<DecryptResult> deleteResults, Options options)
    {
        if (options.RemoveEncrypted == false)
            return [];

        return deleteResults
            .Where(r => r.DecryptedOk)
            .Select(r => r.Source)
            .ToList();
    }

    private static IReadOnlyList<DeleteResult> Delete(IReadOnlyList<FileSystemInfo> filesToDelete)
    {
        var deleteResults = new ConcurrentBag<DeleteResult>();

        AnsiConsole.Progress()
            .Columns(Output.GetProgressColumns())
            .AutoClear(true)
            .Start(progressContext =>
            {
                var progressTask = progressContext.AddTask("[yellow]Deleting Files\t[/]");
                progressTask.MaxValue = filesToDelete.Count;

                Parallel.ForEach(filesToDelete, job =>
                {
                    var deleteResult = FileService.TryDelete(job);
                    deleteResults.Add(deleteResult);
                    progressTask.Increment(1);
                });

            });

        return deleteResults.ToList();
    }

}