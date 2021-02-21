using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QnapBackupDecryptor.Core
{
    public static class OpenSsl
    {
        public const string SALT_HEADER_TEXT = "Salted__";
        public const int SALT_HEADER_SIZE = 8;
        public const int SALT_SIZE = 8;

        public static Result<bool> IsOpenSslEncrypted(FileInfo file)
        {
            var salt = new byte[SALT_SIZE];
            try
            {
                using var fileStream = file.OpenRead();
                fileStream.Read(salt);
                var startsWithHeaderText = System.Text.Encoding.UTF8.GetString(salt) == SALT_HEADER_TEXT;
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

            // read the salt header
            var salt = new byte[SALT_HEADER_SIZE];
            fileStream.Read(salt);

            // sanity check
            var saltHeader = Encoding.UTF8.GetString(salt);
            if (saltHeader != SALT_HEADER_TEXT)
                return Array.Empty<byte>();

            fileStream.Read(salt, 0, SALT_SIZE);

            return salt;
        }

        private static (byte[] key, byte[] iv) DeriveKeyAndIV(byte[] password, byte[] salt)
        {
            var keyAndIvBytes = new List<byte>(48);

            var currentHash = Array.Empty<byte>();

            using var md5Hash = MD5.Create();
            var gotKeyBytes = false;

            while (gotKeyBytes == false)
            {
                var preHashLength = currentHash.Length + password.Length + salt.Length;
                var preHash = new byte[preHashLength];

                Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                Buffer.BlockCopy(password, 0, preHash, currentHash.Length, password.Length);
                Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + password.Length, salt.Length);

                currentHash = md5Hash.ComputeHash(preHash);
                keyAndIvBytes.AddRange(currentHash);

                if (keyAndIvBytes.Count >= 48)
                    gotKeyBytes = true;
            }

            var key = new byte[32];
            var iv = new byte[16];

            // pull out the key
            keyAndIvBytes.CopyTo(0, key, 0, 32);

            // pull out the IV
            keyAndIvBytes.CopyTo(32, iv, 0, 16);

            md5Hash.Clear();

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

                using var destination = outputFile.OpenWrite();
                using var cryptoStream = new CryptoStream(encryptedFileStream, decryptor, CryptoStreamMode.Read);

                cryptoStream.CopyTo(destination);

                destination.Flush();

                return Result<FileInfo>.OkResult(outputFile);
            }
            catch (Exception ex)
            {
                outputFile.Delete();
                return Result<FileInfo>.ErrorResult("could not decrypt", outputFile, ex);
            }
            finally
            {
                rijndaelManaged.Clear();
                rijndaelManaged.Dispose();
            }

        }

    }

}
