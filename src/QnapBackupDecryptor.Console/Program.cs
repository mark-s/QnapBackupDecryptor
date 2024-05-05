using CommandLine;
using QnapBackupDecryptor.Core;
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

        var password = Prompts.GetPassword(options);

        var stopwatch = Stopwatch.StartNew();

        var decryptJobs = GetDecryptJobs(options);

        var (decryptResults, deleteResults) = DoDecrypt(decryptJobs, options, password);

        stopwatch.Stop();

        Output.ShowResults(decryptResults, deleteResults, options.Verbose, stopwatch.Elapsed);
    }

    private static IReadOnlyList<FileJob> GetDecryptJobs(Options options)
    {
        var decryptJobs = new List<FileJob>();

        // get file list to process
        AnsiConsole.Status()
            .Start("Getting Files...", statusContext =>
            {
                statusContext.Spinner(Spinner.Known.SimpleDots);
                statusContext.SpinnerStyle(Style.Parse("green"));

                decryptJobs.AddRange(
                    JobMaker.GetDecryptJobs(
                        encryptedSource: options.EncryptedSource,
                        decryptedTarget: options.OutputDestination,
                        overwrite: options.Overwrite,
                        includeSubFolders: options.IncludeSubfolders)
                    );
            });

        return decryptJobs;
    }

    private static (IReadOnlyList<DecryptResult> DecryptResults, IReadOnlyList<DeleteResult> DeleteResults) DoDecrypt(IReadOnlyCollection<FileJob> decryptJobs, Options options, byte[] password)
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

                Parallel.ForEach(
                    decryptJobs, 
                    currentJob => DecryptSingleJob(options, password, currentJob, decryptResults, deleteResults, progressTask));
            });

        return (decryptResults.ToList(), deleteResults.ToList());

    }

    private static void DecryptSingleJob(
        Options options, byte[] password, FileJob currentJob, ConcurrentBag<DecryptResult> decryptResults,
        ConcurrentBag<DeleteResult> deleteResults, ProgressTask progressTask)
    {
        if (currentJob.IsValid)
        {
            var decryptionResult = OpenSsl.Decrypt(new FileInfo(currentJob.EncryptedFile.FullName), password, new FileInfo(currentJob.OutputFile.FullName));

            decryptResults.Add(new DecryptResult(currentJob.EncryptedFile, currentJob.OutputFile, decryptionResult.IsSuccess, decryptionResult.ErrorMessage));

            // Delete encrypted file only if success and option chosen
            if (decryptionResult.IsSuccess && options.RemoveEncrypted)
                deleteResults.Add(FileService.TryDelete(currentJob.EncryptedFile));
        }
        else
            decryptResults.Add(new DecryptResult(currentJob.EncryptedFile, currentJob.OutputFile, currentJob.IsValid, currentJob.ErrorMessage));

        progressTask.Increment(decryptResults.Count);
    }
}