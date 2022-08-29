namespace QnapBackupDecryptor.Core;

public static class JobMaker
{
    public static List<FileJob> GetDecryptJobs(string encryptedSource, string decryptedTarget, bool overwrite, bool includeSubFolders)
    {
        if (Directory.Exists(encryptedSource) == false && File.Exists(encryptedSource) == false)
            return new FileJob(new DirectoryInfo(encryptedSource), new FileInfo(decryptedTarget), false, "Source does not exist").ToList();

        var sourceIsFolder = File.GetAttributes(encryptedSource).HasFlag(FileAttributes.Directory);

        bool destIsFolder = false;
        if (Directory.Exists(decryptedTarget))
            destIsFolder = File.GetAttributes(decryptedTarget).HasFlag(FileAttributes.Directory);

        if (sourceIsFolder & destIsFolder == false)
            return new FileJob(new DirectoryInfo(encryptedSource), new FileInfo(decryptedTarget), false, "Cannot write an encrypted folder to a single file").ToList();

        if (sourceIsFolder & destIsFolder)
            return GetFolderToFolderJobs(new DirectoryInfo(encryptedSource), new DirectoryInfo(decryptedTarget), overwrite, includeSubFolders);

        if (destIsFolder)
            return GetFileToFolderJob(new FileInfo(encryptedSource), new DirectoryInfo(decryptedTarget), overwrite).ToList();
        else
            return GetFileToFileJob(new FileInfo(encryptedSource), new FileInfo(decryptedTarget), overwrite).ToList();
    }


    private static FileJob GetFileToFileJob(FileInfo encrytedFile, FileInfo outputFile, bool overwrite)
    {
        if (encrytedFile.Exists == false)
            return new FileJob(encrytedFile, outputFile, false, "Encrypted file doesn't exist");

        if (outputFile.Exists & overwrite == false)
            return new FileJob(encrytedFile, outputFile, false, "Output file already exists, use --overwrite to overwrite files.");

        if (outputFile.Exists & outputFile.Attributes.HasFlag(FileAttributes.ReadOnly))
            return new FileJob(encrytedFile, outputFile, false, "Cannot write to output file - it's ReadOnly in the file system.");

        if (OpenSsl.IsOpenSslEncrypted(encrytedFile) == false)
            return new FileJob(encrytedFile, outputFile, false, "File is not encrypted with the OpenSSL method.");

        return new FileJob(encrytedFile, outputFile, true, string.Empty);
    }

    private static FileJob GetFileToFolderJob(FileInfo encrytedFile, DirectoryInfo outputFolder, bool overwrite)
    {
        var outputFile = new FileInfo(Path.Combine(outputFolder.FullName, encrytedFile.Name));
        return GetFileToFileJob(encrytedFile, outputFile, overwrite);
    }

    private static List<FileJob> GetFolderToFolderJobs(DirectoryInfo encrytedFolder, DirectoryInfo outputFolder, bool overwrite, bool includeSubfolders)
    {
        if (encrytedFolder.Exists == false)
            return new FileJob(encrytedFolder, outputFolder, false, "Encrypted folder doesn't exist").ToList();

        if (outputFolder.Exists & outputFolder.Attributes.HasFlag(FileAttributes.ReadOnly))
            return new FileJob(encrytedFolder, outputFolder, false, "Cannot write to output folder - it's ReadOnly in the file system.").ToList();

        return encrytedFolder
            .EnumerateFiles("*.*", includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .AsParallel()
            .Select(encrytedFile => GetFileToFolderJob(encrytedFile, outputFolder, overwrite))
            .ToList();
    }



}