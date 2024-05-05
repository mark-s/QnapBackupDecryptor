namespace QnapBackupDecryptor.Core;

public sealed record DecryptResult(FileSystemInfo Source, FileSystemInfo Dest, bool Success, string ErrorMessage);