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

            // get the password byes
            var password = Prompts.GetPassword(options);

            var decryptResults = new ConcurrentBag<DecryptResult>();
            var deleteResults = new ConcurrentBag<DeleteResult>();
            var decryptJobs = new List<FileJob>();

            var sw = Stopwatch.StartNew();

            // get file list to process
            AnsiConsole.Status()
                .Start("Getting Files...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.SimpleDots);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    decryptJobs = JobMaker.GetDecryptJobs(options.EncryptedSource, options.OutputDestination, options.Overwrite, options.IncludeSubfolders);
                });

            // decrypt & delete if requested
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn(Spinner.Known.SimpleDots)
                })
                .Start(ctx =>
                {
                    var progressTask = ctx.AddTask("[green]Decrypting Files[/]");

                    Parallel.ForEach(
                        decryptJobs,
                        job =>
                        {
                            if (job.IsValid == false)
                                decryptResults.Add(new DecryptResult(job.EncryptedFile, job.OutputFile, job.IsValid, job.ErrorMessage));
                            else
                            {
                                var decryptionResult = OpenSsl.Decrypt(new FileInfo(job.EncryptedFile.FullName), password, new FileInfo(job.OutputFile.FullName));
                                decryptResults.Add(new DecryptResult(job.EncryptedFile, job.OutputFile, decryptionResult.IsSuccess, decryptionResult.ErrorMessage));

                                // Delete encrypted file only if success and option chosen
                                if (decryptionResult.IsSuccess && options.RemoveEncrypted)
                                    deleteResults.Add(FileService.TryDelete(job.EncryptedFile));

                                progressTask.Increment(((double)decryptResults.Count / (double)decryptJobs.Count) * (double)100);
                            }
                        });
                });

            sw.Stop();

            Output.ShowResults(decryptResults, deleteResults, options.Verbose, sw.Elapsed);
        }

    }

}
