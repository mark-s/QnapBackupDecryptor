using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace QnapBackupDecryptor.Core
{

    public static class OpenSsl
    {
        public const string SALT_HEADER_TEXT = "Salted__";
        public const int SALT_HEADER_SIZE = 8;
        public const int SALT_SIZE = 8;
        public const int KEY_SIZE = 32;
        public const int IV_SIZE = 16;

        public static Result<bool> IsOpenSslEncrypted(FileInfo file)
        {
            var saltHeaderBytes = new byte[SALT_HEADER_SIZE];
            try
            {
                using var fileStream = file.OpenRead();
                fileStream.Read(saltHeaderBytes);
                var startsWithHeaderText = System.Text.Encoding.UTF8.GetString(saltHeaderBytes) == SALT_HEADER_TEXT;
                return Result<bool>.OkResult(startsWithHeaderText);
            }
            catch (Exception ex)
            {
                return Result<bool>.ErrorResult(ex.Message, false);
            }
        }

        public static Result<FileInfo> Decrypt(FileInfo encryptedFile, byte[] password, FileInfo outputFile)
        {
            var salt = GetSalt(encryptedFile);

            var (key, iv) = DeriveKeyAndIV(password, salt);

            if (key == null || iv == null || salt == null)
                return Result<FileInfo>.ErrorResult("Key / IV / Salt is invalid", outputFile);

            return DecryptFile(encryptedFile, key, iv, outputFile);
        }

        private static byte[] GetSalt(FileInfo encryptedFile)
        {
            using var fileStream = encryptedFile.OpenRead();

            var salt = new byte[SALT_SIZE];
            fileStream.Position = SALT_HEADER_SIZE;
            fileStream.Read(salt, 0, SALT_SIZE);

            return salt;
        }

        // This inspired by https://gist.github.com/scottlowe/1411917/bdb474d03da42b6bd46e339ef03780f5301b14d7
        private static (byte[] key, byte[] iv) DeriveKeyAndIV(byte[] password, byte[] salt)
        {
            var keyAndIvBytes = new List<byte>(KEY_SIZE + IV_SIZE);

            var currentHash = Array.Empty<byte>();

            using var md5Hash = MD5.Create();

            while (keyAndIvBytes.Count < (KEY_SIZE + IV_SIZE))
            {
                var preHashLength = currentHash.Length + password.Length + salt.Length;
                var preHash = new byte[preHashLength];

                Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                Buffer.BlockCopy(password, 0, preHash, currentHash.Length, password.Length);
                Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + password.Length, salt.Length);

                currentHash = md5Hash.ComputeHash(preHash);
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
            var rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128,
                Key = key,
                IV = iv,
                Padding = PaddingMode.PKCS7
            };

            try
            {
                var decryptor = rijndaelManaged.CreateDecryptor();

                using var encryptedFileStream = encryptedFile.OpenRead();
                encryptedFileStream.Position = SALT_HEADER_SIZE + SALT_SIZE;

                using (var destination = outputFile.OpenWrite())
                using (var cryptoStream = new CryptoStream(encryptedFileStream, decryptor, CryptoStreamMode.Read))
                {
                    outputFile.Attributes |= FileAttributes.Hidden;
                    cryptoStream.CopyTo(destination);
                }

                outputFile.Attributes -= FileAttributes.Hidden;

                return Result<FileInfo>.OkResult(outputFile);
            }
            catch (Exception ex)
            {
                if (outputFile.TryDelete())
                    return Result<FileInfo>.ErrorResult("could not decrypt", outputFile, ex);
                else
                    return Result<FileInfo>.ErrorResult("could not decrypt and failed to delete temp decrypt file.", outputFile, ex);
            }
            finally
            {
                rijndaelManaged.Clear();
                rijndaelManaged.Dispose();
            }
        }
    }
}
