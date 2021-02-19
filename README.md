# Qnap Backup Decryptor

A tool to decrypt QNAP NAS encrypted backup files.

This will decrypt **backup** files (not sync files) created using the [QNAP Hybrid Backup Sync](https://www.qnap.com/en-uk/software/hybrid-backup-sync) tool.


![See it in action](https://raw.githubusercontent.com/mark-s/QnapBackupDecryptor/master/Images/ExampleDecrypt.gif)

## Installation

Binaries for Windows, Linux and Mac, are available in [Releases](https://github.com/mark-s/QnapBackupDecryptor/releases).

The QnapBackupDecryptor-FD files are *Framework dependent* and require an install of .NET 5 to be installed on the system. Available from [here](https://dotnet.microsoft.com/download/dotnet/5.0).
If installing .NET is not an option, the QnapBackupDecryptor-SC files are larger, but do not require a .NET 5 install.

## Quickstart

**Decrypt a Folder, prompt for password and see the complete output file list**

This is the same as the eample gif above.

- Windows
`QnapBackupDecryptor.exe -e c:\Files\Enc -d c:\Files\Dec --verbose`
- Linux
`QnapBackupDecryptor -e ./Files/Enc -d ./Files/Enc --verbose`

**Decrypt a Folder and see the complete list of files, but specify the password**

- Windows
`QnapBackupDecryptor.exe -e c:\Files\Enc -d c:\Files\Dec --verbose -p Pa$$w0rd`
- Linux
`QnapBackupDecryptor -e ./Files/Enc -d ./Files/Enc --verbose -p Pa$$w0rd`

**Decrypt a Folder and overwrite any duplicate files in the destination**

- Windows
`QnapBackupDecryptor.exe -e c:\Files\Enc -d c:\Files\Dec --verbose --overwrite`
- Linux
`QnapBackupDecryptor -e ./Files/Enc -d ./Files/Enc --verbose --overwrite`

**Decrypt a Folder and delete and successfully decrypted source files**

WARNING: This will delete the Encrypted files if they are successfully decrypted.
Ensure you have backups as the files will not be recoverable!

This will prompt for confirmation.

- Windows
`QnapBackupDecryptor.exe -e c:\Files\Enc -d c:\Files\Dec --verbose --removeencrypted`
- Linux
`QnapBackupDecryptor -e ./Files/Enc -d ./Files/Enc --verbose --removeencrypted`

**Decrypt a single file to a folder**

- Windows
`QnapBackupDecryptor.exe -e c:\Files\Enc\Encrypted.jpg -d c:\Files\Dec --verbose`
- Linux
`QnapBackupDecryptor -e ./Files/Enc/Encrypted.jpg -d ./Files/Enc --verbose`

**Decrypt a single file and specify the new name**

- Windows
`QnapBackupDecryptor.exe -e c:\Files\Enc\Encrypted.jpg -d c:\Files\Dec\Decrypted.jpg --verbose`
- Linux
`QnapBackupDecryptor -e ./Files/Enc/Encrypted.jpg -d ./Files/Enc/Decrypted.jpg --verbose`

## Available Options

|Short|Long| |Default|
|------------- |------------- |------------- |------------- |
|-e|--encrypted|Required. Encrypted file or folder||
|-d|--decrypted|Required. Where to place the decrypted file(s)||
|-p|--password|Password|will prompt|
|-s|--subfolders|Include Subfolders|false|
|-r|--removeencrypted|Delete encrypted files (will prompt)|false|
|-v|--verbose|Set output to verbose|false|
|-o|--overwrite|Overwrite file(s) in output|false|
| |--help|Display this help screen.||
| |--version|Display version information.||

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE.md](LICENSE.md) file for details

# Disclaimer

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
