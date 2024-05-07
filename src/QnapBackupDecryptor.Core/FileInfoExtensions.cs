namespace QnapBackupDecryptor.Core;

internal static class FileInfoExtensions
{
    internal static bool TryDelete(this FileInfo fileInfo)
    {
        try
        {
            fileInfo.Refresh();
            if (fileInfo.Exists)
                fileInfo.Delete();

            return true;
        }
        catch
        {
            return false;
        }
    }
}