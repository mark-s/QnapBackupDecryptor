using System.Runtime.InteropServices;

namespace QnapBackupDecryptor.Core;

internal static class FileHelpers
{
    internal static void HideFile(FileSystemInfo file)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            file.Attributes |= FileAttributes.Hidden;
    }

    internal static void ShowFile(FileSystemInfo file)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            file.Attributes -= FileAttributes.Hidden;
    }
}