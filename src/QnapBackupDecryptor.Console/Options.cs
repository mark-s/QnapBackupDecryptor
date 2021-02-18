using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace QnapBackupDecryptor.Console
{
    public class Options
    {
        [Option('p', "password", Required = false, HelpText = "Password")]
        public string Password { get; set; }

        [Option('e', "encrypted", Required = true, HelpText = "Encrypted file or folder")]
        public string EncryptedSource { get; set; }

        [Option('d', "decrypted", Required = true, HelpText = "Where to place the decrypted file(s)")]
        public string OutputDestination { get; set; }

        [Option('s', "subfolders", Required = false, HelpText = "Include Subfolders (default: false)")]
        public bool IncludeSubfolders { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose")]
        public bool Verbose { get; set; }

        [Option('o', "overwrite", Required = false, HelpText = "Overwrite file(s) in output (default: false)")]
        public bool Overwrite { get; set; }

        [Usage(ApplicationAlias = "QnapBackupDecryptor.exe")]
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Decrypt a single file", new Options { EncryptedSource = "file.bin", OutputDestination = "out.bin", Password = "Pa$$word" });
                yield return new Example("Decrypt a folder", new Options { EncryptedSource = "./encryptedfolder", OutputDestination = "./decryptedfolder", Password = "Pa$$word" });
                yield return new Example("Decrypt a folder, overwriting files in the destination", new Options { EncryptedSource = "./encryptedfolder", OutputDestination = "./decryptedfolder", Password = "Pa$$word", Overwrite = true });
                yield return new Example("Decrypt a folder and all subfolders, and prompt for password", new Options { EncryptedSource = "./encryptedfolder", OutputDestination = "./decryptedfolder", IncludeSubfolders = true });
            }
        }

    }

}
