using QnapBackupDecryptor.Core.Models;

namespace QnapBackupDecryptor.Core;

public class DecryptorService
{
    public static (DecryptResult decryptResult, DeleteResult? deleteResult) Decrypt(
        bool removeEncrypted,
        byte[] password,
        FileJob job,
        Action<double> progressUpdate)
    {

        DecryptResult decrypted;
        DeleteResult? deleted =null;

        if (job.IsValid)
        {
            var decryptionResult = OpenSsl.Decrypt(
                encryptedFile: new FileInfo(job.EncryptedFile.FullName),
                password: password,
                outputFile: new FileInfo(job.OutputFile.FullName));

            decrypted = new DecryptResult(job.EncryptedFile, job.OutputFile, decryptionResult.IsSuccess, decryptionResult.ErrorMessage);

            // Delete encrypted file only if success and option chosen
            if (decryptionResult.IsSuccess && removeEncrypted)
                deleted = FileService.TryDelete(job.EncryptedFile);
        }
        else
        {
            decrypted = new DecryptResult(job.EncryptedFile, job.OutputFile, job.IsValid, job.ErrorMessage);
        }

        progressUpdate(1);

        return (decrypted, deleted);

    }
}
