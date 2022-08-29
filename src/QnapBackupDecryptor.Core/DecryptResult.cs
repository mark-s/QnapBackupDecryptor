namespace QnapBackupDecryptor.Core;

public record DecryptResult(FileSystemInfo Source, FileSystemInfo Dest, bool Success, string ErrorMessage);