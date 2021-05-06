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
public record ConnectorPackage(string Id, string Version, MemoryStream? Stream = null)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="ct"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task Extract(string destination, CancellationToken ct = default)
    {
        if (Stream == null)
            throw new ArgumentNullException(nameof(Stream), "Missing package content");

        using var packageReader = new PackageArchiveReader(Stream);

        var files = await packageReader.GetPackageFilesAsync(PackageSaveMode.Files, ct);

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            var entry       = packageReader.GetEntry(file);
            var extractPath = Path.Combine(destination, entry.Name);
            entry.ExtractToFile(extractPath);
        }
    }
}

}
