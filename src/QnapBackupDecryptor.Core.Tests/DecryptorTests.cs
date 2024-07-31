using System.Text;

namespace QnapBackupDecryptor.Core.Tests;

[TestFixture]
public class DecryptorTests
{
    private const string VALID_TEST_PASSWORD = "wisLUBIMyBNcnvo3eDMS";

    [Test]
    public void OpenSSLDecrypt_ValidPassword_OkResult()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo("decrypted.txt");

        // Act 
        var passwordBytes = Encoding.UTF8.GetBytes(VALID_TEST_PASSWORD);
        var sslDecrypt = OpenSsl.Decrypt(encryptedFile, passwordBytes, outputFile);

        // Assert
        sslDecrypt.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void OpenSSLDecrypt_Text_ValidPassword_SuccessResult()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo("decrypted.txt");

        // Act 
        var passwordBytes = Encoding.UTF8.GetBytes(VALID_TEST_PASSWORD);
        var sslDecrypt = OpenSsl.Decrypt(encryptedFile, passwordBytes, outputFile);

        // Assert
        var decryptedText = File.ReadAllLines(sslDecrypt.Data.FullName);
        decryptedText.Length.ShouldBe(2);
        decryptedText[0].ShouldStartWith("line1: this is a plaintext file");
        decryptedText[1].ShouldStartWith("line2: End!");
    }

    [Test]
    public void OpenSSLDecrypt_Text_ValidPassword_ErrorResult()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo("decrypted.txt");

        // Act 
        var passwordBytes = "Invalid password!"u8.ToArray();
        var decryptResult = OpenSsl.Decrypt(encryptedFile, passwordBytes, outputFile);

        // Assert
        decryptResult.IsError.ShouldBeTrue();
    }

    [Test]
    public void OpenSSLDecrypt_Binary_InvalidPassword_ErrorResult()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.jpg"));
        var decryptedFile = new FileInfo(Path.Combine("TestFiles", "decrypted.jpg"));

        // Act 
        var passwordBytes = "Invalid password!"u8.ToArray();
        var decryptionResult = OpenSsl.Decrypt(encryptedFile, passwordBytes, decryptedFile);
        
        // Assert
        decryptionResult.IsError.ShouldBeTrue();
    }

}