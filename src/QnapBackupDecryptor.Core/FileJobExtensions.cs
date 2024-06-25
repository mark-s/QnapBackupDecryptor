using QnapBackupDecryptor.Core.Models;

namespace QnapBackupDecryptor.Core;

internal static class FileJobExtensions
{
    internal static IReadOnlyList<DecryptJob> ToJobs(this DecryptJob job)
        => [job];
}