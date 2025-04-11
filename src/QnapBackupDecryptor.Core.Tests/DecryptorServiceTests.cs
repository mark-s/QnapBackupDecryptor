using System.Text;
using QnapBackupDecryptor.Core.Models;

namespace QnapBackupDecryptor.Core.Tests;

[TestFixture]
public class DecryptorServiceTests
{
    private const string TEST_PASSWORD = "testPassword123";
    private FileInfo _encryptedFile;
    private FileInfo _outputFile;

    [SetUp]
    public void Setup()
    {
        _encryptedFile = new FileInfo(Path.Combine("TestFiles", "test.encrypted"));
        _outputFile = new FileInfo(Path.Combine("TestFiles", "test.decrypted"));
    }

    [Test]
    public void TryDecrypt_ValidJob_ReturnsSuccessResult()
    {
        // Arrange
        var file = new FileInfo(Path.Combine("TestFiles", Guid.NewGuid().ToString()));
        File.WriteAllText(file.FullName, "test test test");

        OpenSsl.Encrypt(file, Encoding.UTF8.GetBytes(TEST_PASSWORD), _encryptedFile);
        var validJob = DecryptJob.Valid(_encryptedFile, _outputFile);
        var passwordBytes = Encoding.UTF8.GetBytes(TEST_PASSWORD);

        // Act
        DecryptResult result = DecryptorService.TryDecrypt(passwordBytes, validJob);

        // Assert
        result.DecryptedOk.ShouldBeTrue();
        result.Source.ShouldBe(_encryptedFile);
        result.Dest.ShouldBe(_outputFile);
    }

    [Test]
    public void TryDecrypt_InvalidJob_ReturnsFailureWithError()
    {
        // Arrange
        const string ERROR_MESSAGE = "Invalid job error";
        var invalidJob = DecryptJob.Invalid(_encryptedFile, _outputFile, ERROR_MESSAGE);
        var passwordBytes = Encoding.UTF8.GetBytes(TEST_PASSWORD);

        // Act
        var result = DecryptorService.TryDecrypt(passwordBytes, invalidJob);

        // Assert
        result.ErrorMessage.ShouldBe(ERROR_MESSAGE);
        result.DecryptedOk.ShouldBeFalse();
        result.Source.ShouldBe(_encryptedFile);
        result.Dest.ShouldBe(_outputFile);
    }

    [Test]
    public void TryDecrypt_DecryptionFails_ReturnsFailureWithError()
    {
        // Arrange
        var validJob = DecryptJob.Valid(_encryptedFile, _outputFile);
        var invalidPassword = "wrong password"u8.ToArray();

        // Act
        var result = DecryptorService.TryDecrypt(invalidPassword, validJob);

        // Assert
        result.DecryptedOk.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNullOrEmpty();
        result.Source.ShouldBe(_encryptedFile);
        result.Dest.ShouldBe(_outputFile);
    }
}
