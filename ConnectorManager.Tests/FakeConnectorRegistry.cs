using System.Collections.Generic;
using System.IO;
using System.Threading;
using NuGet.Packaging;
using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.ConnectorManagement.Tests;

public class FakeConnectorRegistry : IConnectorRegistry
{
    public static readonly IReadOnlyList<ConnectorMetadata> Connectors = new List<ConnectorMetadata>
    {
        new("Reductech.Sequence.Connectors.FileSystem", "0.13.0-a.master.2105052158"),
        new("Reductech.Sequence.Connectors.FileSystem", "0.13.0"),
        new("Reductech.Sequence.Connectors.StructuredData", "0.7.0"),
        new("Reductech.Sequence.Connectors.StructuredData", "0.8.0"),
        new("Reductech.Sequence.Connectors.StructuredData", "0.13.0"),
        new("Reductech.Sequence.Connectors.Nuix", "0.13.0-a.master.2105052200"),
        new("Reductech.Sequence.Connectors.Nuix", "0.13.0-beta.1"),
        new("Reductech.Sequence.Connectors.Nuix", "0.13.0-beta.2")
    }.AsReadOnly();

    public Task<ICollection<ConnectorMetadata>> Find(
        string search,
        bool prerelease = false,
        CancellationToken ct = default) => Task.FromResult(
        (ICollection<ConnectorMetadata>)Connectors
            .Where(
                c => c.Id.Contains(search, StringComparison.OrdinalIgnoreCase)
                  && (prerelease || !c.Version.Contains('-'))
            )
            .GroupBy(c => c.Id)
            .Select(g => g.Last())
            .ToList()
    );

    public Task<bool>
        Exists(string id, string? version = null, CancellationToken ct = default) =>
        Task.FromResult(
            Connectors.Any(
                c => c.Id.Equals(id, StringComparison.Ordinal)
                  && (version == null || c.Version.Equals(version))
            )
        );

    public Task<ICollection<string>> GetVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default) => Task.FromResult(
        (ICollection<string>)Connectors.Where(
                c => c.Id.Equals(id, StringComparison.Ordinal)
                  && (prerelease || !c.Version.Contains('-'))
            )
            .Select(c => c.Version)
            .ToList()
    );

    public Task<ConnectorPackage> GetConnectorPackage(
        string id,
        string version,
        CancellationToken ct = default) => Task.FromResult(
        new ConnectorPackage(
            new ConnectorMetadata(id, version),
            new PackageArchiveReader(
                File.OpenRead(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "Reductech.Sequence.Connectors.filesystem.0.13.0.nupkg"
                    )
                )
            )
        )
    );
}
