namespace QnapBackupDecryptor.Core;

public readonly record struct QnapEncryptionCheckResult(bool IsQnapEncrypted, int EncryptionVersion, bool Compressable);