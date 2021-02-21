using System.IO;

namespace QnapBackupDecryptor.Core
{
    public record DeleteResult(FileSystemInfo ToDelete, bool DeletedOk, string ErrorMessage);
}