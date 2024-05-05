namespace QnapBackupDecryptor.Core;

public sealed record FileJob(FileSystemInfo EncryptedFile, FileSystemInfo OutputFile, bool IsValid, string ErrorMessage);