using NUnit.Framework;
using Shouldly;

namespace QnapBackupDecryptor.Core.Tests;

[TestFixture]
public class JobMakerTests
{

    [Test]
    public void GetDecryptJobs_TwoFolders_SourceDoesntExist_Error()
    {
        // Arrange
        // Act 
        var result = JobMaker.GetDecryptJobs("/somefolder1", "/somefolder2", false, false);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsValid.ShouldBeFalse();
        result[0].ErrorMessage.ShouldBe("Source does not exist");
    }

    [Test]
    public void GetDecryptJobs_TwoFolders_DestDoesntExist_ErrorAssumesFileOutput()
    {
        // Arrange
        string path = Directory.GetCurrentDirectory();
        string destPath = Path.Combine(path, "somefolder");

        // Act 
        var result = JobMaker.GetDecryptJobs(path, destPath, false, false);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsValid.ShouldBeFalse();
        result[0].ErrorMessage.ShouldBe("Cannot write an encrypted folder to a single file");
    }

    [Test]
    public void GetDecryptJobs_EncryptedFileExists_TargetDoesntExist_ProducesValidJob()
    {
        // Arrange
        var encFile = Path.Combine("TestFiles", "encrypted.txt");
        var decFile = Path.Combine("TestFiles", "dec.txt");

        // Act 
        var result = JobMaker.GetDecryptJobs(encFile, decFile, false, false);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsValid.ShouldBeTrue();
        result[0].EncryptedFile.FullName.ShouldEndWith(encFile);
        result[0].OutputFile.FullName.ShouldEndWith(decFile);
    }

    [Test]
    public void GetDecryptJobs_EncryptedFileDoesntExist_TargetExists_OverwriteTrue_ProducesInValidJob()
    {
        // Arrange
        var encFile = Path.Combine("TestFiles", "encrypted.txt_NO");
        var decFile = Path.GetTempFileName();

        // Act 
        var result = JobMaker.GetDecryptJobs(encFile, decFile, overwrite: true, false);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsValid.ShouldBeFalse();
        result[0].ErrorMessage.ShouldBe("Source does not exist");
    }

    [Test]
    public void GetDecryptJobs_EncryptedFileExists_TargetExists_OverwriteFalse_ProducesInValidJob()
    {
        // Arrange
        var encFile = Path.Combine("TestFiles", "encrypted.txt");
        var decFile = Path.GetTempFileName();

        // Act 
        var result = JobMaker.GetDecryptJobs(encFile, decFile, overwrite: false, false);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsValid.ShouldBeFalse();
        result[0].ErrorMessage.ShouldBe("Output file already exists, use --overwrite to overwrite files.");
    }

}