namespace QnapBackupDecryptor.Core.Models;

public sealed record DecryptResult(FileSystemInfo Source, FileSystemInfo Dest, bool DecryptedOk, string ErrorMessage);