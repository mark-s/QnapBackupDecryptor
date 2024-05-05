namespace QnapBackupDecryptor.Core;

public sealed record DeleteResult(FileSystemInfo ToDelete, bool DeletedOk, string ErrorMessage);