using System.Security.Cryptography;
using QnapBackupDecryptor.Core.Models;

namespace QnapBackupDecryptor.Core;

public static class OpenSsl
{
    private const string SALT_HEADER_TEXT = "Salted__";
    private const int SALT_HEADER_SIZE = 8;
    private const int SALT_SIZE = 8;
    private const int KEY_SIZE = 32;
    private const int IV_SIZE = 16;
    private const int COMBINED_KEY_AND_IV_LENGTH = KEY_SIZE + IV_SIZE;

    public static Result<bool> IsOpenSslEncrypted(FileInfo file)
    {
        var saltHeaderBytes = new byte[SALT_HEADER_SIZE];
        try
        {
            using var fileStream = file.OpenRead();
            _ = fileStream.Read(saltHeaderBytes);
            var startsWithHeaderText = System.Text.Encoding.UTF8.GetString(saltHeaderBytes) == SALT_HEADER_TEXT;
            return Result<bool>.OkResult(startsWithHeaderText);
        }
        catch (Exception ex)
        {
            return Result<bool>.ErrorResult(ex.Message, false);
        }
    }

    // create openSSl encrypted file
    public static Result<FileInfo> Encrypt(FileInfo file, byte[] password, FileInfo outputFile)
    {
        var salt = GenerateSecureRandomBytes(SALT_SIZE);
        
        var (key, iv) = DeriveKeyAndIV(password, salt);
        if (key.Length == 0 || iv.Length == 0 || salt.Length == 0)
            return Result<FileInfo>.ErrorResult("Key / IV / Salt is invalid", outputFile);
        return EncryptFile(file, key, iv, salt, outputFile);
    }

    private static Result<FileInfo> EncryptFile(FileInfo file, byte[] key, byte[] iv, byte[] salt, FileInfo outputFile)
    {
        var couldOpenOutputFileForWrite = false;
        using var aes = CreateAes(key, iv);
        try
        {
            using var source = file.OpenRead();
            using var destination = outputFile.OpenWrite();
            couldOpenOutputFileForWrite = true;
            var encryptor = aes.CreateEncryptor();
            using var cryptoStream = new CryptoStream(destination, encryptor, CryptoStreamMode.Write);
            // write the salt header
            var saltHeader = System.Text.Encoding.UTF8.GetBytes(SALT_HEADER_TEXT);
            destination.Write(saltHeader, 0, SALT_HEADER_SIZE);
            // write the salt
            destination.Write(salt, 0, SALT_SIZE);
            FileHelpers.HideFile(outputFile);
            source.CopyTo(cryptoStream);
            FileHelpers.ShowFile(outputFile);
            return Result<FileInfo>.OkResult(outputFile);
        }
        catch (Exception ex)
        {
            if (couldOpenOutputFileForWrite == false)
                return Result<FileInfo>.ErrorResult("could not encrypt - could not write to output file", outputFile, ex);
            if (outputFile.TryDelete())
                return Result<FileInfo>.ErrorResult("could not encrypt", outputFile, ex);
            else
                return Result<FileInfo>.ErrorResult("could not encrypt and failed to delete temp encrypt file.", outputFile, ex);
        }
    }

    public static Result<FileInfo> Decrypt(FileInfo encryptedFile, byte[] password, FileInfo outputFile)
    {
        encryptedFile.Refresh();
        if (encryptedFile.Exists == false)
            return Result<FileInfo>.ErrorResult($"Encrypted file {outputFile.FullName} does not exist", outputFile);

        // check encrypted file size is at least the size of the salt header and salt
        if (encryptedFile.Length < SALT_HEADER_SIZE + SALT_SIZE)
            return Result<FileInfo>.ErrorResult($"Encrypted file {outputFile.FullName} is too small to be encrypted", outputFile);

        var salt = GetSalt(encryptedFile);

        var (key, iv) = DeriveKeyAndIV(password, salt);

        if (key.Length == 0 || iv.Length == 0 || salt.Length == 0)
            return Result<FileInfo>.ErrorResult("Key / IV / Salt is invalid", outputFile);

        return DecryptFile(encryptedFile, key, iv, outputFile);
    }

    private static byte[] GetSalt(FileInfo encryptedFile)
    {
        using var fileStream = encryptedFile.OpenRead();

        var salt = new byte[SALT_SIZE];
        fileStream.Position = SALT_HEADER_SIZE;
        _ = fileStream.Read(salt, 0, SALT_SIZE);

        return salt;
    }

    // This inspired by https://gist.github.com/scottlowe/1411917/bdb474d03da42b6bd46e339ef03780f5301b14d7
    private static (byte[] key, byte[] iv) DeriveKeyAndIV(byte[] password, byte[] salt)
    {
        var keyAndIvBytes = new List<byte>(COMBINED_KEY_AND_IV_LENGTH);

        var currentHash = Array.Empty<byte>();

        while (keyAndIvBytes.Count < COMBINED_KEY_AND_IV_LENGTH)
        {
            var preHashLength = currentHash.Length + password.Length + salt.Length;
            var preHash = new byte[preHashLength];

            Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
            Buffer.BlockCopy(password, 0, preHash, currentHash.Length, password.Length);
            Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + password.Length, salt.Length);

            currentHash = MD5.HashData(preHash);
            keyAndIvBytes.AddRange(currentHash);
        }

        // pull out the key
        var key = new byte[KEY_SIZE];
        keyAndIvBytes.CopyTo(0, key, 0, KEY_SIZE);

        // pull out the IV
        var iv = new byte[IV_SIZE];
        keyAndIvBytes.CopyTo(KEY_SIZE, iv, 0, IV_SIZE);

        return (key, iv);
    }

    private static Result<FileInfo> DecryptFile(FileInfo encryptedFile, byte[] key, byte[] iv, FileInfo outputFile)
    {
        var couldOpenOutputFileForWrite = false;
        using var aes = CreateAes(key, iv);

        try
        {
            using var encryptedFileStream = encryptedFile.OpenRead();
            encryptedFileStream.Position = SALT_HEADER_SIZE + SALT_SIZE;

            using var destination = outputFile.OpenWrite();
            couldOpenOutputFileForWrite = true;

            var decryptor = aes.CreateDecryptor();
            using var cryptoStream = new CryptoStream(encryptedFileStream, decryptor, CryptoStreamMode.Read);

            FileHelpers.HideFile(outputFile);

            cryptoStream.CopyTo(destination);

            FileHelpers.ShowFile(outputFile);

            return Result<FileInfo>.OkResult(outputFile);
        }
        catch (Exception ex)
        {
            if (couldOpenOutputFileForWrite == false)
                return Result<FileInfo>.ErrorResult("could not decrypt - could not write to output file", outputFile, ex);

            if (outputFile.TryDelete())
                return Result<FileInfo>.ErrorResult("could not decrypt", outputFile, ex);
            else
                return Result<FileInfo>.ErrorResult("could not decrypt and failed to delete temp decrypt file.", outputFile, ex);
        }

    }

    private static Aes CreateAes(byte[] key, byte[] iv)
    {
        var aes = Aes.Create();

        aes.Mode = CipherMode.CBC;
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Key = key;
        aes.IV = iv;
        aes.Padding = PaddingMode.PKCS7;

        return aes;
    }

    private static byte[] GenerateSecureRandomBytes(int length)
    {
        var randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return randomBytes;
    }
}