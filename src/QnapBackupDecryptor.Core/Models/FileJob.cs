namespace QnapBackupDecryptor.Core.Models;

public sealed record FileJob(FileSystemInfo EncryptedFile, FileSystemInfo OutputFile, bool IsValid, string ErrorMessage);