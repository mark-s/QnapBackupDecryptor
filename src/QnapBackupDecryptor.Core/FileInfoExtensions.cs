namespace QnapBackupDecryptor.Core;

public static class FileInfoExtensions
{
    public static bool TryDelete(this FileInfo fileInfo)
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