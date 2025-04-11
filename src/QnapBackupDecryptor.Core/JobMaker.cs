using QnapBackupDecryptor.Core.Models;

namespace QnapBackupDecryptor.Core;

public static class JobMaker
{
    public static IReadOnlyList<DecryptJob> CreateDecryptJobs(string encryptedSource, string decryptedTarget, bool overwrite, bool includeSubFolders)
    {
        if (Directory.Exists(encryptedSource) == false && File.Exists(encryptedSource) == false)
            return DecryptJob.Invalid(new DirectoryInfo(encryptedSource), new FileInfo(decryptedTarget), "Source does not exist").ToJobs();

        var sourceIsFolder = IsFolder(encryptedSource);
        var destIsFolder = Directory.Exists(decryptedTarget) && IsFolder(decryptedTarget);

        if (sourceIsFolder & destIsFolder == false)
            return DecryptJob.Invalid(new DirectoryInfo(encryptedSource), new FileInfo(decryptedTarget), "Cannot write an encrypted folder to a single file").ToJobs();

        if (sourceIsFolder & destIsFolder)
            return FolderToFolderJobs(new DirectoryInfo(encryptedSource), new DirectoryInfo(decryptedTarget), overwrite, includeSubFolders);

        if (destIsFolder)
            return FileToFolderJob(new FileInfo(encryptedSource), new DirectoryInfo(decryptedTarget), overwrite).ToJobs();
        else
            return FileToFileJob(new FileInfo(encryptedSource), new FileInfo(decryptedTarget), overwrite).ToJobs();
    }

    private static DecryptJob FileToFileJob(FileInfo encryptedFile, FileInfo outputFile, bool overwrite)
    {
        if (encryptedFile.Exists == false)
            return DecryptJob.Invalid(encryptedFile, outputFile, "Encrypted file doesn't exist");

        if (outputFile.Exists & overwrite == false)
            return DecryptJob.Invalid(encryptedFile, outputFile, "Output file already exists, use --overwrite to overwrite files.");

        if (outputFile.Exists & outputFile.Attributes.HasFlag(FileAttributes.ReadOnly))
            return DecryptJob.Invalid(encryptedFile, outputFile, "Cannot write to output file - it's ReadOnly in the file system.");

        if (OpenSsl.IsOpenSslEncrypted(encryptedFile) == false)
            return DecryptJob.Invalid(encryptedFile, outputFile, "File is not encrypted with the OpenSSL method.");

        return DecryptJob.Valid(encryptedFile, outputFile);
    }

    private static DecryptJob FileToFolderJob(FileInfo encryptedFile, FileSystemInfo outputFolder, bool overwrite)
    {
        var outputFile = new FileInfo(Path.Combine(outputFolder.FullName, encryptedFile.Name));
        return FileToFileJob(encryptedFile, outputFile, overwrite);
    }

    private static IReadOnlyList<DecryptJob> FolderToFolderJobs(DirectoryInfo encryptedFolder, FileSystemInfo outputFolder, bool overwrite, bool includeSubfolders)
    {
        if (encryptedFolder.Exists == false)
            return DecryptJob.Invalid(encryptedFolder, outputFolder, "Encrypted folder doesn't exist").ToJobs();

        if (outputFolder.Exists & outputFolder.Attributes.HasFlag(FileAttributes.ReadOnly))
            return DecryptJob.Invalid(encryptedFolder, outputFolder, "Cannot write to output folder - it's ReadOnly in the file system.").ToJobs();

        return encryptedFolder
            .EnumerateFiles("*.*", includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .AsParallel()
            .Select(encryptedFile => FileToFolderJob(encryptedFile, outputFolder, overwrite))
            .ToList();
    }

    private static bool IsFolder(string encryptedSource)
        => File.GetAttributes(encryptedSource).HasFlag(FileAttributes.Directory);
}