using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace QnapBackupDecryptor.Console;

internal class Options
{
    [Option('p', "password", Required = false, HelpText = "Password")]
    public string? Password { get; init; }

    [Option('e', "encrypted", Required = true, HelpText = "Encrypted file or folder")]
    public string EncryptedSource { get; init; } = null!;

    [Option('d', "decrypted", Required = true, HelpText = "Where to place the decrypted file(s)")]
    public string OutputDestination { get; init; } = null!;

    [Option('s', "subfolders", Required = false, HelpText = "Include Subfolders (default: false)")]
    public bool IncludeSubfolders { get; init; }

    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose")]
    public bool Verbose { get; init; }

    [Option('o', "overwrite", Required = false, HelpText = "Overwrite file(s) in output (default: false)")]
    public bool Overwrite { get; init; }

    [Option('r', "removeencrypted", Required = false, HelpText = "Delete encrypted files when decrypted (default: false)")]
    public bool RemoveEncrypted { get; init; }


    [Usage(ApplicationAlias = "QnapBackupDecryptor")]
    // ReSharper disable once UnusedMember.Global // Used by the console
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Decrypt a single file", new Options { EncryptedSource = "file.bin", OutputDestination = "out.bin", Password = "Pa$$word" });
            yield return new Example("Decrypt a folder", new Options { EncryptedSource = "./encryptedfolder", OutputDestination = "./decryptedfolder", Password = "Pa$$word" });
            yield return new Example("Decrypt a folder, and delete the source encrypted files", new Options { EncryptedSource = "./encryptedfolder", OutputDestination = "./decryptedfolder", RemoveEncrypted = true });
            yield return new Example("Decrypt a folder, overwriting files in the destination", new Options { EncryptedSource = "./encryptedfolder", OutputDestination = "./decryptedfolder", Password = "Pa$$word", Overwrite = true });
            yield return new Example("Decrypt a folder and all subfolders, and prompt for password", new Options { EncryptedSource = "./encryptedfolder", OutputDestination = "./decryptedfolder", IncludeSubfolders = true });
        }
    }

}