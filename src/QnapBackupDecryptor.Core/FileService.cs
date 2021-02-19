using System;
using System.IO;

namespace QnapBackupDecryptor.Core
{
    public record DeleteResult(FileSystemInfo ToDelete, bool DeletedOk, string ErrorMessage);

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


}
