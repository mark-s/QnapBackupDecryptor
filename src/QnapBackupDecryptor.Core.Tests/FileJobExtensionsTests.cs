namespace QnapBackupDecryptor.Core.Tests;

using QnapBackupDecryptor.Core.Models;

[TestFixture]
public class FileJobExtensionsTests
{
    [Test]
    public void ToJobs_ValidDecryptJob_ReturnsSingleElementList()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo(Path.Combine("TestFiles", "output.txt"));
        var job = DecryptJob.Valid(encryptedFile, outputFile);

        // Act
        var result = job.ToJobs();

        // Assert
        result.Count.ShouldBe(1);
        result[0].ShouldBeSameAs(job);
    }

    [Test]
    public void ToJobs_InvalidDecryptJob_ReturnsSingleElementList()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo(Path.Combine("TestFiles", "output.txt"));
        var errorMessage = "Test error message";
        var job = DecryptJob.Invalid(encryptedFile, outputFile, errorMessage);

        // Act
        var result = job.ToJobs();

        // Assert
        result.Count.ShouldBe(1);
        result[0].ShouldBeSameAs(job);
        result[0].IsValid.ShouldBeFalse();
        result[0].ErrorMessage.ShouldBe(errorMessage);
    }

    [Test]
    public void ToJobs_PreservesOriginalJobProperties()
    {
        // Arrange
        var encryptedFile = new FileInfo(Path.Combine("TestFiles", "encrypted.txt"));
        var outputFile = new FileInfo(Path.Combine("TestFiles", "output.txt"));
        var job = DecryptJob.Valid(encryptedFile, outputFile);

        // Act
        var result = job.ToJobs();

        // Assert
        result[0].EncryptedFile.ShouldBe(encryptedFile);
        result[0].OutputFile.ShouldBe(outputFile);
        result[0].IsValid.ShouldBeTrue();
        result[0].ErrorMessage.ShouldBe(string.Empty);
    }
}
