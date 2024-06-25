namespace QnapBackupDecryptor.Core.Models;

public sealed class DecryptJob
{
    public FileSystemInfo EncryptedFile { get; }
    public FileSystemInfo OutputFile { get; }
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    private DecryptJob(FileSystemInfo encryptedFile, FileSystemInfo outputFile, bool isValid, string errorMessage)
    {
        EncryptedFile = encryptedFile;
        OutputFile = outputFile;
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    internal static DecryptJob Invalid(FileSystemInfo encryptedFile, FileSystemInfo outputFile, string errorMessage)
        => new(encryptedFile, outputFile, false, errorMessage);

    internal static DecryptJob Valid(FileSystemInfo encryptedFile, FileSystemInfo outputFile)
        => new(encryptedFile, outputFile, true, string.Empty);

};