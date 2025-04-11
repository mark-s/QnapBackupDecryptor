namespace QnapBackupDecryptor.Core.Tests;

[TestFixture]
public class QnapTests
{
    private FileInfo _validFileV1 = null!;
    private FileInfo _validFileV2Compressed = null!;
    private FileInfo _validFileV2NoCompressed = null!;
    private FileInfo _invalidFile = null!;

    [SetUp]
    public void Setup()
    {
        var testFilesDir = Path.Combine("TestFiles", Guid.NewGuid().ToString());
        Directory.CreateDirectory(testFilesDir);

        _validFileV1 = new FileInfo(Path.Combine(testFilesDir, "validV1.qnap"));
        File.WriteAllBytes(_validFileV1.FullName, "__QCS__"u8.ToArray());

        _validFileV2Compressed = new FileInfo(Path.Combine(testFilesDir, "validV2Compressed.qnap"));
        File.WriteAllBytes(_validFileV2Compressed.FullName, [75, 54, 108, 114, 94, 125, 28, 49, 1, 1, 33, 22, 44, 55]);

        _validFileV2NoCompressed = new FileInfo(Path.Combine(testFilesDir, "validV2NoCompressed.qnap"));
        File.WriteAllBytes(_validFileV2NoCompressed.FullName, [75, 54, 108, 114, 94, 125, 28, 49, 1, 0, 33, 22, 44, 55]);

        _invalidFile = new FileInfo(Path.Combine(testFilesDir, "invalid.qnap"));
        File.WriteAllBytes(_invalidFile.FullName, [1, 2, 3, 4]);
    }

    [TearDown]
    public void TearDown()
    {
        if (_validFileV1.Directory?.Exists == true)
            _validFileV1.Directory.Delete(true);
    }

    [Test]
    public void IsQnapEncrypted_ValidV1File_ReturnsEncryptionVersion1()
    {
        // Act
        var result = Qnap.IsQnapEncrypted(_validFileV1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.IsQnapEncrypted.ShouldBeTrue();
        result.Data.EncryptionVersion.ShouldBe(1);
        result.Data.Compressable.ShouldBeFalse();
    }

    [Test]
    public void IsQnapEncrypted_ValidV2CompressedFile_ReturnsEncryptionVersion2WithCompression()
    {
        // Act
        var result = Qnap.IsQnapEncrypted(_validFileV2Compressed);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.IsQnapEncrypted.ShouldBeTrue();
        result.Data.EncryptionVersion.ShouldBe(2);
        result.Data.Compressable.ShouldBeTrue();
    }

    [Test]
    public void IsQnapEncrypted_ValidV2NoCompressedFile_ReturnsEncryptionVersion2WithoutCompression()
    {
        // Act
        var result = Qnap.IsQnapEncrypted(_validFileV2NoCompressed);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.IsQnapEncrypted.ShouldBeTrue();
        result.Data.EncryptionVersion.ShouldBe(2);
        result.Data.Compressable.ShouldBeFalse();
    }

    [Test]
    public void IsQnapEncrypted_InvalidFile_ReturnsError()
    {
        // Act
        var result = Qnap.IsQnapEncrypted(_invalidFile);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Data.IsQnapEncrypted.ShouldBeFalse();
        result.Data.EncryptionVersion.ShouldBe(0);
        result.Data.Compressable.ShouldBeFalse();
    }

    [Test]
    public void IsQnapEncrypted_FileTooSmall_ReturnsError()
    {
        // Arrange
        var smallFile = new FileInfo(Path.Combine("TestFiles", "small.qnap"));
        File.WriteAllBytes(smallFile.FullName, [1]);

        // Act
        var result = Qnap.IsQnapEncrypted(smallFile);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Data.IsQnapEncrypted.ShouldBeFalse();
        result.Data.EncryptionVersion.ShouldBe(0);
        result.Data.Compressable.ShouldBeFalse();
    }
}
