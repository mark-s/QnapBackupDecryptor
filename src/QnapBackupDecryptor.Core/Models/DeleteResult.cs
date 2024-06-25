namespace QnapBackupDecryptor.Core.Models;

public sealed record DeleteResult(FileSystemInfo FileToDelete, bool DeletedOk, string ErrorMessage);