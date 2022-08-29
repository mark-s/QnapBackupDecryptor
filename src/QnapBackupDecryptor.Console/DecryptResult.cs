namespace QnapBackupDecryptor.Console;

public record DecryptResult(FileSystemInfo Source, FileSystemInfo Dest, bool Success, string ErrorMessage);