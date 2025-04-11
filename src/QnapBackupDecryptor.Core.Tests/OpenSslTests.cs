using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace QnapBackupDecryptor.Core.Tests;

[TestFixture]
public class OpenSslTests
{
    private const string VALID_TEST_PASSWORD = "wisLUBIMyBNcnvo3eDMS";
    private readonly byte[] _validPasswordBytes = Encoding.UTF8.GetBytes(VALID_TEST_PASSWORD);
    private readonly byte[] _invalidPasswordBytes = "Invalid password!"u8.ToArray();

    [Test]
    public void IsOpenSslEncrypted_WhenFileDoesNotExist_ReturnsErrorResult()
    {
        // Arrange
        var nonExistentFile = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

        // Act
        var result = OpenSsl.IsOpenSslEncrypted(nonExistentFile);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Data.ShouldBeFalse();
    }

    [Test]
    public void IsOpenSslEncrypted_WhenFileExists_ButEmptyFile_ReturnsFalse()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var emptyFile = new FileInfo(tempFile);

        try
        {
            // Act
            var result = OpenSsl.IsOpenSslEncrypted(emptyFile);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldBeFalse();
        }
        finally
        {
            // Cleanup
            File.Delete(tempFile);
        }
    }

    [Test]
    public void IsOpenSslEncrypted_WhenFileExistsWithWrongHeader_ReturnsFalse()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "NotSalted");
        var fileWithWrongHeader = new FileInfo(tempFile);

        try
        {
            // Act
            var result = OpenSsl.IsOpenSslEncrypted(fileWithWrongHeader);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldBeFalse();
        }
        finally
        {
            // Cleanup
            File.Delete(tempFile);
        }
    }

    [Test]
    public void Decrypt_WhenFileDoesNotExist_ReturnsErrorResult()
    {
        // Arrange
        var nonExistentFile = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        var outputFile = new FileInfo(Path.Combine(Path.GetTempPath(), "output.txt"));

        // Act
        var result = OpenSsl.Decrypt(nonExistentFile, _validPasswordBytes, outputFile);

        // Assert
        result.IsError.ShouldBeTrue();
        result.ErrorMessage.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public void Decrypt_WithTooShortFile_ReturnsErrorResult()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Salted__"); // Only header, no salt
        var shortFile = new FileInfo(tempFile);
        var outputFile = new FileInfo(Path.Combine(Path.GetTempPath(), "output.txt"));

        try
        {
            // Act
            var result = OpenSsl.Decrypt(shortFile, _validPasswordBytes, outputFile);

            // Assert
            result.IsError.ShouldBeTrue();
        }
        finally
        {
            // Cleanup
            File.Delete(tempFile);
        }
    }

    [Test]
    public void Decrypt_WithReadOnlyOutputFile_ReturnsErrorResult()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputTempFile = Path.GetTempFileName();
        File.SetAttributes(outputTempFile, FileAttributes.ReadOnly);
        var readOnlyOutputFile = new FileInfo(outputTempFile);

        try
        {
            // Act
            var result = OpenSsl.Decrypt(encryptedFile, _validPasswordBytes, readOnlyOutputFile);

            // Assert
            result.IsError.ShouldBeTrue();
        }
        finally
        {
            // Cleanup
            File.SetAttributes(outputTempFile, FileAttributes.Normal);
            File.Delete(outputTempFile);
        }
    }

    [Test]
    public void Decrypt_WithValidCredentials_DecryptsTextFile()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));

        if (outputFile.Exists)
            outputFile.Delete();

        try
        {
            // Act
            var result = OpenSsl.Decrypt(encryptedFile, _validPasswordBytes, outputFile);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            outputFile.Exists.ShouldBeTrue();

            var decryptedContent = File.ReadAllText(outputFile.FullName);
            decryptedContent.ShouldContain("line1: this is a plaintext file");
        }
        finally
        {
            // Cleanup
            if (outputFile.Exists)
                outputFile.Delete();
        }
    }

    [Test]
    public void Decrypt_WithInvalidPassword_ReturnsErrorResult()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.jpg"));
        var outputFile = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));

        try
        {
            // Act
            var result = OpenSsl.Decrypt(encryptedFile, _invalidPasswordBytes, outputFile);

            // Assert
            result.IsError.ShouldBeTrue();
            outputFile.Exists.ShouldBeFalse(); // Should be deleted on error
        }
        finally
        {
            // Cleanup
            if (outputFile.Exists)
                outputFile.Delete();
        }
    }

    [Test]
    public void DeriveKeyAndIV_WithValidInputs_ReturnsCorrectKeySizes()
    {
        // Arrange
        var password = _validPasswordBytes;
        var salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }; // 8 bytes salt

        // Access private method using reflection
        var methodInfo = typeof(OpenSsl).GetMethod("DeriveKeyAndIV",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = methodInfo!.Invoke(null, [password, salt]);

        // Assert
        result.ShouldBeOfType<ValueTuple<byte[], byte[]>>();
        var (key, iv) = ((byte[], byte[]))result;

        key.ShouldNotBeNull();
        iv.ShouldNotBeNull();
        key.Length.ShouldBe(32); // KEY_SIZE
        iv.Length.ShouldBe(16);  // IV_SIZE
    }

    [Test]
    public void GetSalt_WithValidEncryptedFile_Returns8BytesSalt()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));

        // Access private method using reflection
        var methodInfo = typeof(OpenSsl).GetMethod("GetSalt",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = methodInfo!.Invoke(null, [encryptedFile]);

        // Assert
        result.ShouldBeOfType<byte[]>();
        var saltBytes = (byte[])result;
        saltBytes.Length.ShouldBe(8); // SALT_SIZE
    }

    [Test]
    public void CreateAes_WithKeyAndIv_CreatesCorrectAesInstance()
    {
        // Arrange
        var key = new byte[32]; // 32 bytes key
        var iv = new byte[16];  // 16 bytes IV

        // Access private method using reflection
        var methodInfo = typeof(OpenSsl).GetMethod("CreateAes",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = methodInfo!.Invoke(null, [key, iv]);

        // Assert
        var aes = result as Aes;

        aes.ShouldNotBeNull();
        aes.Mode.ShouldBe(CipherMode.CBC);
        aes.KeySize.ShouldBe(256);
        aes.BlockSize.ShouldBe(128);
        aes.Padding.ShouldBe(PaddingMode.PKCS7);
    }

    [Test]
    public void Encrypt_WithValidTextFile_CreatesEncryptedFile()
    {
        // Arrange
        var sourceFile = new FileInfo(Path.Combine("TestFiles", "plaintext.txt"));
        var outputFile = new FileInfo(Path.Combine(Path.GetTempPath(), "encrypted.txt"));

        if (!Directory.Exists("TestFiles"))
            Directory.CreateDirectory("TestFiles");
        
        File.WriteAllText(sourceFile.FullName, "Test content\nSecond line");

        try
        {
            // Act
            var result = OpenSsl.Encrypt(sourceFile, _validPasswordBytes, outputFile);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            outputFile.Exists.ShouldBeTrue();
            outputFile.Length.ShouldBeGreaterThan(16); // At least header + salt

            var isEncrypted = OpenSsl.IsOpenSslEncrypted(outputFile);
            isEncrypted.Data.ShouldBeTrue();
        }
        finally
        {
            // Cleanup
            if (outputFile.Exists) outputFile.Delete();
            if (sourceFile.Exists) sourceFile.Delete();
        }
    }

    [Test]
    public void Encrypt_WithBinaryFile_CreatesEncryptedFile()
    {
        // Arrange
        var sourceFile = new FileInfo(Path.Combine("TestFiles", "sample.bin"));
        var outputFile = new FileInfo(Path.Combine(Path.GetTempPath(), "encrypted.bin"));

        if (!Directory.Exists("TestFiles"))
            Directory.CreateDirectory("TestFiles");
        
        File.WriteAllBytes(sourceFile.FullName, [0x00, 0x01, 0x02, 0x03, 0x04]);

        try
        {
            // Act
            var result = OpenSsl.Encrypt(sourceFile, _validPasswordBytes, outputFile);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            outputFile.Exists.ShouldBeTrue();
        
            var isEncrypted = OpenSsl.IsOpenSslEncrypted(outputFile);
            isEncrypted.Data.ShouldBeTrue();
        }
        finally
        {
            // Cleanup
            if (outputFile.Exists) outputFile.Delete();
            if (sourceFile.Exists) sourceFile.Delete();
        }
    }

    [Test]
    public void Encrypt_WithInvalidOutputPath_ReturnsErrorResult()
    {
        // Arrange
        var sourceFile = new FileInfo(Path.Combine("TestFiles", "plaintext.txt"));
        var outputFile = new FileInfo(Path.Combine("NonExistentFolder", "encrypted.txt"));

        File.WriteAllText(sourceFile.FullName, "Test content");

        try
        {
            // Act
            var result = OpenSsl.Encrypt(sourceFile, _validPasswordBytes, outputFile);

            // Assert
            result.IsError.ShouldBeTrue();
            outputFile.Exists.ShouldBeFalse();
        }
        finally
        {
            // Cleanup
            if (sourceFile.Exists) sourceFile.Delete();
        }
    }

    [Test]
    public void Encrypt_WithEmptyFile_CreatesValidEncryptedFile()
    {
        // Arrange
        var sourceFile = new FileInfo(Path.Combine("TestFiles", "empty.txt"));
        var outputFile = new FileInfo(Path.Combine(Path.GetTempPath(), "encrypted-empty.txt"));

        File.WriteAllText(sourceFile.FullName, "");

        try
        {
            // Act
            var result = OpenSsl.Encrypt(sourceFile, _validPasswordBytes, outputFile);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            outputFile.Exists.ShouldBeTrue();
        
            var isEncrypted = OpenSsl.IsOpenSslEncrypted(outputFile);
            isEncrypted.Data.ShouldBeTrue();
        }
        finally
        {
            // Cleanup
            if (outputFile.Exists) outputFile.Delete();
            if (sourceFile.Exists) sourceFile.Delete();
        }
    }

    [Test]
    public void Encrypt_WithNonExistentFile_ReturnsErrorResult()
    {
        // Arrange
        var sourceFile = new FileInfo(Path.Combine(Path.GetTempPath(), "nonexistent.txt"));
        var outputFile = new FileInfo(Path.Combine(Path.GetTempPath(), "encrypted.txt"));

        // Act
        var result = OpenSsl.Encrypt(sourceFile, _validPasswordBytes, outputFile);

        // Assert
        result.IsError.ShouldBeTrue();
        outputFile.Exists.ShouldBeFalse();
    }

    [Test]
    public void IsOpenSslEncrypted_WithValidEncryptedFile_ReturnsTrue()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));

        // Act
        var result = OpenSsl.IsOpenSslEncrypted(encryptedFile);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Test]
    public void EncryptAndDecrypt_WithTextFile_PreservesContent()
    {
        // Arrange
        var originalContent = "Test content\nWith multiple lines\nAnd some special chars: !@#$%";
        var sourceFile = new FileInfo(Path.Combine(Path.GetTempPath(), "original.txt"));
        var encryptedFile = new FileInfo(Path.Combine(Path.GetTempPath(), "encrypted.txt"));
        var decryptedFile = new FileInfo(Path.Combine(Path.GetTempPath(), "decrypted.txt"));

        File.WriteAllText(sourceFile.FullName, originalContent);

        try
        {
            // Act
            var encryptResult = OpenSsl.Encrypt(sourceFile, _validPasswordBytes, encryptedFile);
            var decryptResult = OpenSsl.Decrypt(encryptedFile, _validPasswordBytes, decryptedFile);

            // Assert
            encryptResult.IsSuccess.ShouldBeTrue();
            decryptResult.IsSuccess.ShouldBeTrue();
        
            var decryptedContent = File.ReadAllText(decryptedFile.FullName);
            decryptedContent.ShouldBe(originalContent);
        }
        finally
        {
            // Cleanup
            if (sourceFile.Exists) sourceFile.Delete();
            if (encryptedFile.Exists) encryptedFile.Delete();
            if (decryptedFile.Exists) decryptedFile.Delete();
        }
    }

    [Test]
    public void EncryptAndDecrypt_WithLargeFile_WorksCorrectly()
    {
        // Arrange
        var sourceFile = new FileInfo(Path.Combine(Path.GetTempPath(), "large.dat"));
        var encryptedFile = new FileInfo(Path.Combine(Path.GetTempPath(), "large.encrypted"));
        var decryptedFile = new FileInfo(Path.Combine(Path.GetTempPath(), "large.decrypted"));
    
        // Create 1MB file
        using (var fs = sourceFile.Create())
        {
            fs.SetLength(1024 * 1024);
        }

        try
        {
            // Act
            var encryptResult = OpenSsl.Encrypt(sourceFile, _validPasswordBytes, encryptedFile);
            var decryptResult = OpenSsl.Decrypt(encryptedFile, _validPasswordBytes, decryptedFile);

            // Assert
            encryptResult.IsSuccess.ShouldBeTrue();
            decryptResult.IsSuccess.ShouldBeTrue();
        
            sourceFile.Length.ShouldBe(decryptedFile.Length);
        }
        finally
        {
            // Cleanup
            if (sourceFile.Exists) sourceFile.Delete();
            if (encryptedFile.Exists) encryptedFile.Delete();
            if (decryptedFile.Exists) decryptedFile.Delete();
        }
    }

    [Test]
    public void EncryptAndDecrypt_WithDifferentPasswordLengths_WorksCorrectly()
    {
        // Arrange
        var content = "Test content";
        var sourceFile = new FileInfo(Path.Combine(Path.GetTempPath(), "original.txt"));
        var encryptedFile = new FileInfo(Path.Combine(Path.GetTempPath(), "encrypted.txt"));
        var decryptedFile = new FileInfo(Path.Combine(Path.GetTempPath(), "decrypted.txt"));

        File.WriteAllText(sourceFile.FullName, content);
        var shortPassword = "short"u8.ToArray();
        var longPassword = "ThisIsAVeryLongPasswordThatShouldStillWork!!!"u8.ToArray();

        try
        {
            // Act & Assert - Short password
            var encryptResult1 = OpenSsl.Encrypt(sourceFile, shortPassword, encryptedFile);
            var decryptResult1 = OpenSsl.Decrypt(encryptedFile, shortPassword, decryptedFile);
        
            encryptResult1.IsSuccess.ShouldBeTrue();
            decryptResult1.IsSuccess.ShouldBeTrue();
            File.ReadAllText(decryptedFile.FullName).ShouldBe(content);

            // Cleanup between tests
            encryptedFile.Delete();
            decryptedFile.Delete();

            // Act & Assert - Long password
            var encryptResult2 = OpenSsl.Encrypt(sourceFile, longPassword, encryptedFile);
            var decryptResult2 = OpenSsl.Decrypt(encryptedFile, longPassword, decryptedFile);
        
            encryptResult2.IsSuccess.ShouldBeTrue();
            decryptResult2.IsSuccess.ShouldBeTrue();
            File.ReadAllText(decryptedFile.FullName).ShouldBe(content);
        }
        finally
        {
            // Cleanup
            if (sourceFile.Exists) sourceFile.Delete();
            if (encryptedFile.Exists) encryptedFile.Delete();
            if (decryptedFile.Exists) decryptedFile.Delete();
        }
    }
}
