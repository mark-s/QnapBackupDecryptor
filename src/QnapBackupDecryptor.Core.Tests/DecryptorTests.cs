using System.Text;

namespace QnapBackupDecryptor.Core.Tests;

[TestFixture]
public class DecryptorTests
{
    private const string TEST_PASSWORD = "wisLUBIMyBNcnvo3eDMS";

    [Test]
    public void OpenSSLDecrypt_ValidPassword_OkResult()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo("decrypted.txt");

        // Act 
        var passwordBytes = Encoding.UTF8.GetBytes(TEST_PASSWORD);
        var sslDecrypt = OpenSsl.Decrypt(encryptedFile, passwordBytes, outputFile);

        // Assert
        sslDecrypt.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void OpenSSLDecrypt_Text()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo("decrypted.txt");

        // Act 
        var passwordBytes = Encoding.UTF8.GetBytes(TEST_PASSWORD);
        var sslDecrypt = OpenSsl.Decrypt(encryptedFile, passwordBytes, outputFile);

        // Assert
        var decryptedText = File.ReadAllLines(sslDecrypt.Data.FullName);
        decryptedText.Length.ShouldBe(2);
        decryptedText[0].ShouldStartWith("line1: this is a plaintext file");
        decryptedText[1].ShouldStartWith("line2: End!");
    }

    //[Test]
    //public void OpenSSLDecrypt_Binary()
    //{
    //    // Arrange
    //    var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.jpg"));
    //    var decryptedFile = new FileInfo(Path.Combine("TestFiles", "decrypted.jpg"));

    //    // Act 
    //    var passwordBytes = Encoding.UTF8.GetBytes(TEST_PASSWORD);
    //    var decrypted = OpenSsl.Decrypt(encryptedFile, passwordBytes, decryptedFile);

    //    // Assert
    //    var decryptedText = File.ReadAllText(decrypted.Data.Name);
    //    decryptedText.ShouldBe("line1: this is a plaintext file\r\nline2: End!\r\n");
    //}

}