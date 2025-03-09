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

}
