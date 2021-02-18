using NUnit.Framework;
using Shouldly;
using System.IO;
using System.Text;

namespace QnapBackupDecryptor.Core.Tests
{

    [TestFixture]
    public class DecryptorTests
    {
        private const string PASSWORD = "wisLUBIMyBNcnvo3eDMS";


        [Test]
        public void OpenSSLDecrypt_Text()
        {
            // Arrange
            var encryptedFile = new FileInfo(@"TestFiles/encrypted.txt");
            var decryptedFile = new FileInfo(@"TestFiles/plaintext.txt");

            // Act 
            var passwordBytes = Encoding.UTF8.GetBytes(PASSWORD);
            var sslDecrypt = OpenSsl.Decrypt(encryptedFile, passwordBytes, decryptedFile);

            // Assert
            var decryptedText = File.ReadAllLines(sslDecrypt.Data.FullName);
            decryptedText.Length.ShouldBe(2);
            decryptedText[0].ShouldStartWith("line1: this is a plaintext file");
            decryptedText[1].ShouldStartWith("line2: End!");
        }

        [Test]
        public void OpenSSLDecrypt_Binary()
        {
            // Arrange
            var encryptedFile = new FileInfo(@"TestFiles/encrypted.jpg");
            var decryptedFile = new FileInfo(@"TestFiles/decrypted.jpg");


            // Act 
            var passwordBytes = Encoding.UTF8.GetBytes(PASSWORD);
            var decrpyted = OpenSsl.Decrypt(encryptedFile, passwordBytes, decryptedFile);


            // Assert
            //var decryptedText = File.ReadAllText(decrpyted.FullName);
            //decryptedText.ShouldBe("line1: this is a plaintext file\r\nline2: End!\r\n");
        }

    }
}
