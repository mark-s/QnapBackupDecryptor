﻿using System.Collections.Generic;
using System.IO;

namespace QnapBackupDecryptor.Core
{
    public record FileJob(FileSystemInfo EncryptedFile, FileSystemInfo OutputFile, bool IsValid, string ErrorMessage);

    public static class FileJobExtensions
    {

        public static List<FileJob> ToList(this FileJob job)
        => new List<FileJob> { job };
    }

}
