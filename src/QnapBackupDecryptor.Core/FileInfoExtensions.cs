namespace QnapBackupDecryptor.Core;

internal static class FileInfoExtensions
{
    internal static bool TryDelete(this FileInfo fileInfo)
    {
        try
        {
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