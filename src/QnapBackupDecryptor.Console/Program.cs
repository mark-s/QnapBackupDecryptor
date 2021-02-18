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

        private static void Run(Options opts)
        {
            var password = Prompts.GetPassword(opts);

            var sw = Stopwatch.StartNew();

            var decryptJobs = JobMaker.GetDecryptJob(opts.EncryptedSource, opts.OutputDestination, opts.Overwrite, opts.IncludeSubfolders);

            ConcurrentBag<DecryptResult> results = new ConcurrentBag<DecryptResult>();
            Parallel.ForEach(decryptJobs,
                job =>
                {
                    if (job.Success == false)
                        results.Add(new DecryptResult(job.EncryptedFile, job.OutputFile, job.Success, job.ErrorMessage));
                    else
                    {
                        var result = OpenSsl.Decrypt(new FileInfo(job.EncryptedFile.FullName), password, new FileInfo(job.OutputFile.FullName));
                        results.Add(new DecryptResult(job.EncryptedFile, job.OutputFile, result.IsSuccess, job.ErrorMessage));
                    }
                });

            sw.Stop();
            Output.ShowResults(results, opts.Verbose, sw.Elapsed);
        }

    }

    public record DecryptResult(FileSystemInfo Source, FileSystemInfo Dest, bool Success, string ErrorMessage);


}
