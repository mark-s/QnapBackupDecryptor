using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QnapBackupDecryptor.Core
{
    public record FileJob(FileSystemInfo EncryptedFile, FileSystemInfo OutputFile, bool IsValid, string ErrorMessage);

    public static class JobMaker
    {
        public static List<FileJob> GetDecryptJobs(string encryptedSource, string decryptedTarget, bool overwrite, bool includeSubFolders)
        {
            if (Directory.Exists(encryptedSource) == false & File.Exists(encryptedSource) == false)
                return new List<FileJob> { new FileJob(new DirectoryInfo(encryptedSource), new FileInfo(decryptedTarget), false, "Source does not exist") };

            var sourceIsFolder = File.GetAttributes(encryptedSource).HasFlag(FileAttributes.Directory);

            bool destIsFolder = false;
            if (Directory.Exists(decryptedTarget))
                destIsFolder = File.GetAttributes(decryptedTarget).HasFlag(FileAttributes.Directory);

            if (sourceIsFolder & destIsFolder == false)
                return new List<FileJob> { new FileJob(new DirectoryInfo(encryptedSource), new FileInfo(decryptedTarget), false, "Cannot write an encrypted folder to a single file") };

            if (sourceIsFolder & destIsFolder)
                return GetJobs(new DirectoryInfo(encryptedSource), new DirectoryInfo(decryptedTarget), overwrite, includeSubFolders);

            if (destIsFolder)
                return new List<FileJob> { GetJob(new FileInfo(encryptedSource), new DirectoryInfo(decryptedTarget), overwrite) };
            else
                return new List<FileJob> { GetJob(new FileInfo(encryptedSource), new FileInfo(decryptedTarget), overwrite) };
        }


        private static FileJob GetJob(FileInfo encrytedFile, FileInfo outputFile, bool overwrite)
        {
            if (encrytedFile.Exists == false)
                return new FileJob(encrytedFile, outputFile, false, "Encrypted file doesn't exist");

            if (outputFile.Exists & overwrite == false)
                return new FileJob(encrytedFile, outputFile, false, "Output file already exists, use --overwrite to overwrite files.");

            if (outputFile.Exists & outputFile.Attributes == FileAttributes.ReadOnly)
                return new FileJob(encrytedFile, outputFile, false, "Cannot write to output file - it's ReadOnly in the file system.");

            if (OpenSsl.IsOpenSslEncrypted(encrytedFile) == false)
                return new FileJob(encrytedFile, outputFile, false, "File is not encrypted with the OpenSSL method.");

            return new FileJob(encrytedFile, outputFile, true, string.Empty);
        }

        private static FileJob GetJob(FileInfo encrytedFile, DirectoryInfo outputFolder, bool overwrite)
        {
            var outputFile = new FileInfo(Path.Combine(outputFolder.FullName, encrytedFile.Name));
            return GetJob(encrytedFile, outputFile, overwrite);
        }

        private static List<FileJob> GetJobs(DirectoryInfo encrytedFolder, DirectoryInfo outputFolder, bool overwrite, bool includeSubfolders)
        {
            if (encrytedFolder.Exists == false)
                return new List<FileJob> { new FileJob(encrytedFolder, outputFolder, false, "Encrypted folder doesn't exist") };

            if (outputFolder.Exists & outputFolder.Attributes == FileAttributes.ReadOnly)
                return new List<FileJob> { new FileJob(encrytedFolder, outputFolder, false, "Cannot write to output folder - it's ReadOnly in the file system.") };

            var fileInfos = encrytedFolder.EnumerateFiles("*.*", includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            return fileInfos
                .AsParallel()
                .Select(encrytedFile => GetJob(encrytedFile, outputFolder, overwrite))
                .ToList();
        }



    }
}
