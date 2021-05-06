using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// 
/// </summary>
public record ConnectorMetadata(string Id, string Version) { }

/// <summary>
/// 
/// </summary>
public sealed record ConnectorPackage
    (ConnectorMetadata Metadata, PackageArchiveReader Package) : IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="ct"></param>
    public async Task Extract(string destination, CancellationToken ct = default)
    {
        var files = await Package.GetPackageFilesAsync(PackageSaveMode.Files, ct);

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            var entry       = Package.GetEntry(file);
            var extractPath = Path.Combine(destination, entry.Name);
            entry.ExtractToFile(extractPath);
        }
    }

    /// <inheritdoc />
    public void Dispose() => Package.Dispose();
}

}
