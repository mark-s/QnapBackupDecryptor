using QnapBackupDecryptor.Core.Models;

namespace QnapBackupDecryptor.Core;

public static class Qnap
{
    private static readonly byte[] QnapFilePrefixV1 = "__QCS__"u8.ToArray();

    private static readonly byte[] QnapFilePrefixV2 = [75, 54, 108, 114, 94, 125, 28, 49, 1];

    private const byte QNAP_V2_COMPRESSED = 1;

    public static Result<QnapEncryptionCheckResult> IsQnapEncrypted(FileInfo file)
    {
        if(file.Exists == false || file.Length < QnapFilePrefixV1.Length)
            return Result<QnapEncryptionCheckResult>.ErrorResult("File does not exist or is too small", new QnapEncryptionCheckResult(false, 0, false));

        if (IsV1Encrypted(file))
            return Result<QnapEncryptionCheckResult>.OkResult(new QnapEncryptionCheckResult(true, 1, false));

        if (IsV2Encrypted(file))
            return Result<QnapEncryptionCheckResult>.OkResult(new QnapEncryptionCheckResult(true, 2, IsCompressed(file)));

        return Result<QnapEncryptionCheckResult>.OkResult(new QnapEncryptionCheckResult(false, 0, false));
    }

    private static Result<bool> IsV1Encrypted(FileInfo file)
    {
        var saltHeaderBytes = new byte[QnapFilePrefixV1.Length];
        try
        {
            using var fileStream = file.OpenRead();
            _ = fileStream.Read(saltHeaderBytes);
            return Result<bool>.OkResult(saltHeaderBytes.SequenceEqual(QnapFilePrefixV1));
        }
        catch (Exception ex)
        {
            return Result<bool>.ErrorResult(ex.Message, false);
        }
    }

    private static Result<bool> IsV2Encrypted(FileInfo file)
    {
        var saltHeaderBytes = new byte[QnapFilePrefixV2.Length];
        try
        {
            using var fileStream = file.OpenRead();
            _ = fileStream.Read(saltHeaderBytes);
            return Result<bool>.OkResult(saltHeaderBytes.SequenceEqual(QnapFilePrefixV2));
        }
        catch (Exception ex)
        {
            return Result<bool>.ErrorResult(ex.Message, false);
        }
    }

    private static Result<bool> IsCompressed(FileInfo file)
    {
        if (IsV2Encrypted(file) == false)
            return Result<bool>.OkResult(false);
        
        try
        {
            using var fileStream = file.OpenRead();
            fileStream.Seek(9, SeekOrigin.Begin);
            return Result<bool>.OkResult(fileStream.ReadByte() == QNAP_V2_COMPRESSED);
        }
        catch (Exception ex)
        {
            return Result<bool>.ErrorResult(ex.Message, false);
        }
    }

}