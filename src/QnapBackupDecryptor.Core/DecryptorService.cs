using QnapBackupDecryptor.Core.Models;

namespace QnapBackupDecryptor.Core;

public static class DecryptorService
{
    public static DecryptResult TryDecrypt(byte[] password, DecryptJob job)
    {
        if (job.IsValid == false)
            return new DecryptResult(job.EncryptedFile, job.OutputFile, job.IsValid, job.ErrorMessage);
      
        var decryptionResult = OpenSsl.Decrypt(
            encryptedFile: new FileInfo(job.EncryptedFile.FullName),
            password: password,
            outputFile: new FileInfo(job.OutputFile.FullName));

        return new DecryptResult(job.EncryptedFile, job.OutputFile, decryptionResult.IsSuccess, decryptionResult.ErrorMessage);
    }
}
