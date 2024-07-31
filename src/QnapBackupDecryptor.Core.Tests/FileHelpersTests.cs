namespace QnapBackupDecryptor.Core.Tests;

[TestFixture]
public class FileHelpersTests
{
    [Test]
    public void IsOpenSslEncrypted_OpenSslFile_Binary_True()
    {
        // Arrange
        var oseFile = new FileInfo(Path.Combine("TestFiles", "encrypted.jpg"));

        // Act 
        var result = OpenSsl.IsOpenSslEncrypted(oseFile);

        // Assert
        result.Data.ShouldBeTrue();
    }

    [Test]
    public void IsOpenSslEncrypted_OpenSslFile_Text_True()
    {
        // Arrange
        var oseFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        
        // Act 
        var result = OpenSsl.IsOpenSslEncrypted(oseFile);

        // Assert
        result.Data.ShouldBeTrue();
    }

    [Test]
    public void IsOpenSslEncrypted_NotOpenSslFile_False()
    {
        // Arrange
        var oseFile = new FileInfo(Path.Combine("TestFiles", "plaintext.txt"));

        // Act 
        var result = OpenSsl.IsOpenSslEncrypted(oseFile);

        // Assert
        result.Data.ShouldBeFalse();
    }
}