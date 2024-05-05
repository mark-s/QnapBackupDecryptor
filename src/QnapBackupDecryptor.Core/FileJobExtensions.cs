namespace QnapBackupDecryptor.Core;

internal static class FileJobExtensions
{
    internal static List<FileJob> ToList(this FileJob job)
        => new List<FileJob>() { job };
}