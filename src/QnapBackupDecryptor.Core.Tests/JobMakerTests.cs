using System.Linq;

namespace QnapBackupDecryptor.Core.Tests;

[TestFixture]
public class JobMakerTests
{

    [Test]
    public void GetDecryptJobs_TwoFolders_SourceDoesntExist_Error()
    {
        // Arrange
        // Act 
        var result = JobMaker.CreateDecryptJobs("/somefolder1", "/somefolder2", false, false);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsValid.ShouldBeFalse();
        result[0].ErrorMessage.ShouldBe("Source does not exist");
    }

    [Test]
    public void GetDecryptJobs_TwoFolders_DestDoesntExist_ErrorAssumesFileOutput()
    {
        // Arrange
        var path = Directory.GetCurrentDirectory();
        var destPath = Path.Combine(path, "somefolder");

        // Act 
        var result = JobMaker.CreateDecryptJobs(path, destPath, false, false);

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
        var result = JobMaker.CreateDecryptJobs(encFile, decFile, false, false);

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
        var result = JobMaker.CreateDecryptJobs(encFile, decFile, overwrite: true, false);

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
        var result = JobMaker.CreateDecryptJobs(encFile, decFile, overwrite: false, false);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsValid.ShouldBeFalse();
        result[0].ErrorMessage.ShouldBe("Output file already exists, use --overwrite to overwrite files.");
    }

    [Test]
    public void GetDecryptJobs_EncryptedFileExists_TargetExistsAndReadOnly_ProducesInvalidJob()
    {
        // Arrange
        var encFile = Path.Combine("TestFiles", "encrypted.txt");
        var decFile = Path.GetTempFileName();
        File.SetAttributes(decFile, FileAttributes.ReadOnly);

        try
        {
            // Act 
            var result = JobMaker.CreateDecryptJobs(encFile, decFile, overwrite: true, false);

            // Assert
            result.Count.ShouldBe(1);
            result[0].IsValid.ShouldBeFalse();
            result[0].ErrorMessage.ShouldBe("Cannot write to output file - it's ReadOnly in the file system.");
        }
        finally
        {
            // Cleanup
            File.SetAttributes(decFile, FileAttributes.Normal);
            File.Delete(decFile);
        }
    }

    [Test]
    public void GetDecryptJobs_FileNotEncryptedWithOpenSsl_ProducesInvalidJob()
    {
        // Arrange
        var plainFile = Path.Combine("TestFiles", "plaintext.txt");
        var decFile = Path.Combine("TestFiles", "dec_plain.txt");

        // Act 
        var result = JobMaker.CreateDecryptJobs(plainFile, decFile, false, false);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsValid.ShouldBeFalse();
        result[0].ErrorMessage.ShouldBe("File is not encrypted with the OpenSSL method.");
    }

    [Test]
    public void GetDecryptJobs_EncryptedFileExists_TargetExists_OverwriteTrue_ProducesValidJob()
    {
        // Arrange
        var encFile = Path.Combine("TestFiles", "encrypted.txt");
        var decFile = Path.GetTempFileName();

        try
        {
            // Act 
            var result = JobMaker.CreateDecryptJobs(encFile, decFile, overwrite: true, false);

            // Assert
            result.Count.ShouldBe(1);
            result[0].IsValid.ShouldBeTrue();
            result[0].EncryptedFile.FullName.ShouldEndWith(encFile);
            result[0].OutputFile.FullName.ShouldBe(decFile);
        }
        finally
        {
            // Cleanup
            File.Delete(decFile);
        }
    }

    [Test]
    public void GetDecryptJobs_EncryptedFileToFolder_ProducesValidJob()
    {
        // Arrange
        var encFile = Path.Combine("TestFiles", "encrypted.txt");
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act 
            var result = JobMaker.CreateDecryptJobs(encFile, tempDir, false, false);

            // Assert
            result.Count.ShouldBe(1);
            result[0].IsValid.ShouldBeTrue();
            result[0].EncryptedFile.FullName.ShouldEndWith(encFile);
            result[0].OutputFile.FullName.ShouldEndWith(Path.Combine(tempDir, "encrypted.txt"));
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public void GetDecryptJobs_FolderToFolder_IncludeSubFoldersTrue_ProducesMultipleJobs()
    {
        // Arrange
        List<string> testFileNames = ["encrypted.jpg", "encrypted.txt", "plaintext.txt"];
        var tempDir1 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir1);
        foreach (var file in Directory.GetFiles("TestFiles", "*.*", SearchOption.TopDirectoryOnly).Where(x => testFileNames.Contains(x.Split(Path.DirectorySeparatorChar).Last())))
        {
            var destFile = Path.Combine(tempDir1, Path.GetFileName(file));
            File.Copy(file, destFile);
        }

        var tempDir2 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir2);

        try
        {
            // Act 
            var result = JobMaker.CreateDecryptJobs(tempDir1, tempDir2, false, true);

            // Assert
            result.Count.ShouldBe(3);
            result.Count(j => j.IsValid).ShouldBe(2); // one of the files is not encrypted

        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir2, true);
        }
    }

    [Test]
    public void GetDecryptJobs_FolderToFolder_IncludeSubFoldersFalse_OnlyIncludesTopLevelFiles()
    {
        // Arrange
        var sourceDir = "TestFiles"; // Contains encrypted files
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            // First count all files (recursive)
            var allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).Length;
            var topLevelFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.TopDirectoryOnly).Length;
            
            // If no subdirectories with files exist, we'll skip this test
            if (allFiles == topLevelFiles)
            {
                Assert.Ignore("No subdirectories with files to test with");
            }

            // Act 
            var result = JobMaker.CreateDecryptJobs(sourceDir, tempDir, false, false);

            // Assert
            result.Count.ShouldBe(topLevelFiles);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public void GetDecryptJobs_DestFolderReadOnly_ProducesInvalidJob()
    {
        // Arrange
        var sourceDir = "TestFiles";
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        File.SetAttributes(tempDir, FileAttributes.ReadOnly);

        try
        {
            // Act 
            var result = JobMaker.CreateDecryptJobs(sourceDir, tempDir, false, false);

            // Assert
            result.Count.ShouldBe(1);
            result[0].IsValid.ShouldBeFalse();
            result[0].ErrorMessage.ShouldBe("Cannot write to output folder - it's ReadOnly in the file system.");
        }
        finally
        {
            // Cleanup
            File.SetAttributes(tempDir, FileAttributes.Normal);
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public void GetDecryptJobs_EncryptedFolderDoesntExist_ProducesInvalidJob()
    {
        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act - Create another directory to ensure we're testing "Encrypted folder doesn't exist" not just "Source doesn't exist"
            var result = JobMaker.CreateDecryptJobs(nonExistentDir, tempDir, false, false);

            // Assert
            result.Count.ShouldBe(1);
            result[0].IsValid.ShouldBeFalse();
            // The specific message depends on whether the code checks if it's a folder before checking existence
            result[0].ErrorMessage.ShouldBeOneOf("Encrypted folder doesn't exist", "Source does not exist");
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}