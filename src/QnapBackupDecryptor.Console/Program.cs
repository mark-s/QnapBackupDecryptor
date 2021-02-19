using CommandLine;
using QnapBackupDecryptor.Core;
using System.Collections.Concurrent;
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
            if (Prompts.EnsureDeleteWanted(options) == false)
                return;

            var password = Prompts.GetPassword(options);

            var sw = Stopwatch.StartNew();

            var decryptJobs = JobMaker.GetDecryptJobs(options.EncryptedSource, options.OutputDestination, options.Overwrite, options.IncludeSubfolders);

            var decryptResults = new ConcurrentBag<DecryptResult>();
            var deleteResults = new ConcurrentBag<DeleteResult>();

            Parallel.ForEach(decryptJobs,
                job =>
                {
                    if (job.IsValid == false)
                        decryptResults.Add(new DecryptResult(job.EncryptedFile, job.OutputFile, job.IsValid, job.ErrorMessage));
                    else
                    {
                        var result = OpenSsl.Decrypt(new FileInfo(job.EncryptedFile.FullName), password, new FileInfo(job.OutputFile.FullName));
                        decryptResults.Add(new DecryptResult(job.EncryptedFile, job.OutputFile, result.IsSuccess, result.ErrorMessage));

                        // Delete encrypted file only if success and option chosen
                        if (result.IsSuccess && options.RemoveEncrypted)
                            deleteResults.Add(FileService.TryDelete(job.EncryptedFile));
                    }
                });

            sw.Stop();

            Output.ShowResults(decryptResults, deleteResults, options.Verbose, sw.Elapsed);
        }


    }




}
