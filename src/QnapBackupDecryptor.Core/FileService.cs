namespace QnapBackupDecryptor.Core;

public static class FileService
{
    public static DeleteResult TryDelete(FileSystemInfo toDelete)
    {
        try
        {
            toDelete.Delete();
            return new DeleteResult(toDelete, true, string.Empty);
        }
        catch (Exception ex)
        {
            return new DeleteResult(toDelete, false, ex.Message);
        }
    }

}