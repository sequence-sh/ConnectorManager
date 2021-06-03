using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Stores connector metadata.
/// </summary>
public record ConnectorMetadata(string Id, string Version) { }

/// <summary>
/// Connector metadata which includes a downloaded connector package.
/// </summary>
public sealed record ConnectorPackage
    (ConnectorMetadata Metadata, PackageArchiveReader Package) : IDisposable
{
    private static readonly string[] FlattenPaths = { "lib/net5.0/", "contentFiles/any/any/" };

    /// <summary>
    /// Extract the connector to the destination directory.
    /// </summary>
    /// <param name="fileSystem">The file system to use.</param>
    /// <param name="destination">The destination directory.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task Extract(
        IFileSystem fileSystem,
        string destination,
        CancellationToken ct = default)
    {
        var files = await Package.GetPackageFilesAsync(PackageSaveMode.Files, ct);

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            var entry    = Package.GetEntry(file);
            var filePath = entry.FullName.TrimStart('/');

            foreach (var rm in FlattenPaths)
                if (filePath.StartsWith(rm))
                    filePath = filePath.Remove(0, rm.Length);

            var extractPath = fileSystem.Path.Combine(destination, filePath);

            fileSystem.FileInfo.FromFileName(extractPath).Directory.Create();

            entry.ExtractToFile(fileSystem, extractPath);
        }
    }

    /// <inheritdoc />
    public void Dispose() => Package.Dispose();
}

}
