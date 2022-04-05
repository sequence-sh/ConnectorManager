using System.Data;
using System.IO;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.ConnectorManagement;

/// <summary>
/// Connector management using a nuget feed.
/// </summary>
public class ConnectorRegistry : IConnectorRegistry
{
    private readonly ILogger _logger;
    private readonly ConnectorManagerSettings _settings;

    /// <summary>
    /// Max results to return from the remote registry when using Find.
    /// </summary>
    public int FindResults { get; set; } = 100;

    /// <summary>
    /// Create a new connector registry with the specified settings.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="connectorManagerSettings"></param>
    public ConnectorRegistry(
        Microsoft.Extensions.Logging.ILogger<ConnectorRegistry> logger,
        ConnectorManagerSettings connectorManagerSettings)
    {
        _logger = connectorManagerSettings.EnableNuGetLog
            ? new LoggerBridge<ConnectorRegistry>(logger)
            : new NullLogger();

        _settings = connectorManagerSettings;
    }

    /// <inheritdoc />
    public async Task<ICollection<ConnectorMetadata>> Find(
        string search,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        List<ConnectorMetadata> results = new();

        foreach (var registry in _settings.Registries)
        {
            var resource = await GetPrivateResource<PackageSearchResource>(registry, ct);

            var searchResult = await resource.SearchAsync(
                search,
                new SearchFilter(prerelease),
                skip: 0,
                take: FindResults,
                _logger,
                ct
            );

            results.AddRange(
                searchResult.Select(
                        p => new ConnectorMetadata(
                            p.Identity.Id,
                            p.Identity.Version.ToNormalizedString()
                        )
                    )
                    .ToList()
            );
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<bool> Exists(
        string id,
        string? version = null,
        CancellationToken ct = default)
    {
        var allVersions = await GetVersion(id, true, ct);

        if (allVersions.Count == 0)
            return false;

        return version == null || allVersions.Contains(version);
    }

    /// <inheritdoc />
    public async Task<ICollection<string>> GetVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        List<string> results = new();

        foreach (var registry in _settings.Registries)
        {
            var resource = await GetPrivateResource<FindPackageByIdResource>(registry, ct);
            var cache    = new SourceCacheContext();
            var versions = await resource.GetAllVersionsAsync(id, cache, _logger, ct);

            results.AddRange(
                versions.Where(v => prerelease || !v.IsPrerelease)
                    .Select(v => v.ToNormalizedString())
                    .ToList()
            );
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<ConnectorPackage> GetConnectorPackage(
        string id,
        string version,
        CancellationToken ct = default)
    {
        NuGetVersion nugetVersion;

        try
        {
            nugetVersion = NuGetVersion.Parse(version);
        }
        catch (ArgumentException)
        {
            throw new VersionNotFoundException($"Could not parse version: {version}");
        }

        var ms = new MemoryStream();

        foreach (var registry in _settings.Registries)
        {
            var resource = await GetPrivateResource<FindPackageByIdResource>(registry, ct);
            var cache    = new SourceCacheContext();
            await resource.CopyNupkgToStreamAsync(id, nugetVersion, ms, cache, _logger, ct);

            if (ms.Length > 0)
                break;
        }

        if (ms.Length == 0)
            throw new ArgumentException($"Can't find connector {id} ({version})");

        var packageReader = new PackageArchiveReader(ms);

        var packageIdentity = await packageReader.GetIdentityAsync(ct);

        return new ConnectorPackage(
            new ConnectorMetadata(packageIdentity.Id, packageIdentity.Version.ToNormalizedString()),
            packageReader
        );
    }

    private async Task<T> GetPrivateResource<T>(
        ConnectorRegistryEndpoint registry,
        CancellationToken ct)
        where T : class, INuGetResource
    {
        SourceRepository repository;

        if (!string.IsNullOrEmpty(registry.User)
         || !string.IsNullOrEmpty(registry.Token))
        {
            var source = new PackageSource(registry.Uri)
            {
                Credentials = new PackageSourceCredential(
                    source: registry.Uri,
                    username: registry.User,
                    passwordText: registry.Token,
                    isPasswordClearText: true,
                    validAuthenticationTypesText: null
                )
            };

            repository = Repository.Factory.GetCoreV3(source);
        }
        else
        {
            repository = Repository.Factory.GetCoreV3(registry.Uri);
        }

        return await repository.GetResourceAsync<T>(ct);
    }
}
