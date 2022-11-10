using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;

namespace Sequence.ConnectorManagement;

/// <summary>
/// Modified from
/// https://github.com/dotnet/corefx/blob/master/src/System.IO.Compression.ZipFile/src/System/IO/Compression/ZipFileExtensions.ZipArchiveEntry.Extract.cs
/// to allow for IFileSystem injection
/// </summary>
public static class ZipArchiveEntryExtensions
{
    /// <summary>
    /// Creates a file on the file system with the entry?s contents and the specified name.
    /// The last write time of the file is set to the entry?s last write time.
    /// This method does allows overwriting of an existing file with the same name.
    /// </summary>
    ///
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="ArgumentException">destinationFileName is a zero-length string, contains only whitespace,
    /// or contains one or more invalid characters as defined by InvalidPathChars. -or- destinationFileName specifies a directory.</exception>
    /// <exception cref="ArgumentNullException">destinationFileName is null.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    /// <exception cref="DirectoryNotFoundException">The path specified in destinationFileName is invalid
    /// (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">destinationFileName exists and overwrite is false.
    /// -or- An I/O error has occurred.
    /// -or- The entry is currently open for writing.
    /// -or- The entry has been deleted from the archive.</exception>
    /// <exception cref="NotSupportedException">destinationFileName is in an invalid format
    /// -or- The ZipArchive that this entry belongs to was opened in a write-only mode.</exception>
    /// <exception cref="InvalidDataException">The entry is missing from the archive or is corrupt and cannot be read
    /// -or- The entry has been compressed using a compression method that is not supported.</exception>
    /// <exception cref="ObjectDisposedException">The ZipArchive that this entry belongs to has been disposed.</exception>
    /// <param name="source">The zip archive entry to extract a file from.</param>
    /// <param name="fileSystem">The file system</param>
    /// <param name="destinationFileName">The name of the file that will hold the contents of the entry.
    /// The path is permitted to specify relative or absolute path information.
    /// Relative path information is interpreted as relative to the current working directory.</param>
    /// <param name="overwrite">True to indicate overwrite.</param>
    public static void ExtractToFile(
        this ZipArchiveEntry source,
        IFileSystem fileSystem,
        string destinationFileName,
        bool overwrite = false)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (destinationFileName == null)
            throw new ArgumentNullException(nameof(destinationFileName));

        // Rely on FileStream's ctor for further checking destinationFileName parameter
        FileMode fMode = overwrite ? FileMode.Create : FileMode.CreateNew;

        using (Stream fs = fileSystem.FileStream.Create(
                   destinationFileName,
                   fMode,
                   FileAccess.Write,
                   FileShare.None,
                   bufferSize: 0x1000,
                   useAsync: false
               ))
        {
            using (Stream es = source.Open())
                es.CopyTo(fs);
        }

        fileSystem.File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
    }
}
