namespace QnapBackupDecryptor.Core.Models;

public sealed record DeleteResult(FileSystemInfo ToDelete, bool DeletedOk, string ErrorMessage);