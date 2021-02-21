using CommandLine;
using QnapBackupDecryptor.Core;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace QnapBackupDecryptor.Console
{
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

            DoDecrypt(decryptJobs, options, password, stopwatch);
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

                    decryptJobs = JobMaker.GetDecryptJobs(options.EncryptedSource, options.OutputDestination, options.Overwrite, options.IncludeSubfolders);
                });

            return decryptJobs;
        }

        private static void DoDecrypt(IReadOnlyList<FileJob> decryptJobs, Options options, byte[] password, Stopwatch sw)
        {
            AnsiConsole.Progress()
                .Columns(Output.GetProgressColumns())
                .AutoClear(true)
                .Start(progressContext =>
                {
                    var progressTask = progressContext.AddTask("[green]Decrypting Files[/]");

                    var decryptResults = new ConcurrentBag<DecryptResult>();
                    var deleteResults = new ConcurrentBag<DeleteResult>();

                    Parallel.ForEach(decryptJobs, currentJob =>
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

                        progressTask.Increment(((double)decryptResults.Count / (double)decryptJobs.Count) * (double)100);
                    });

                    sw.Stop();

                    Output.ShowResults(decryptResults, deleteResults, options.Verbose, sw.Elapsed);
                });
        }

    }
}
