using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Connector management using a nuget feed.
/// </summary>
public class ConnectorRegistry : IConnectorRegistry
{
    private readonly ILogger _logger;
    private readonly ConnectorRegistrySettings _settings;

    /// <summary>
    /// Max results to return from the remote registry when using Find.
    /// </summary>
    public int FindResults { get; set; } = 100;

    /// <summary>
    /// Create a new connector registry with the specified settings.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="connectorRegistrySettings"></param>
    public ConnectorRegistry(
        Microsoft.Extensions.Logging.ILogger<ConnectorRegistry> logger,
        ConnectorRegistrySettings connectorRegistrySettings)
    {
        _logger   = new LoggerBridge<ConnectorRegistry>(logger);
        _settings = connectorRegistrySettings;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IPackageSearchMetadata>> Find(
        string search,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        var resource = await GetPrivateResource<PackageSearchResource>(ct);

        var results = await resource.SearchAsync(
            search,
            new SearchFilter(prerelease),
            skip: 0,
            take: FindResults,
            _logger,
            ct
        );

        return results;
    }

    /// <inheritdoc />
    public async Task<bool> Exists(string id, NuGetVersion version, CancellationToken ct = default)
    {
        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();
        return await resource.DoesPackageExistAsync(id, version, cache, _logger, ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IPackageSearchMetadata>> GetMetadata(
        string id,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        var resource = await GetPrivateResource<PackageMetadataResource>(ct);
        var cache    = new SourceCacheContext();

        var connectorVersions = await resource.GetMetadataAsync(
            id,
            prerelease,
            includeUnlisted: false,
            cache,
            _logger,
            ct
        );

        return connectorVersions;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<NuGetVersion>> GetVersion(
        string id,
        CancellationToken ct = default)
    {
        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();

        var connectorVersions = await resource.GetAllVersionsAsync(
            id,
            cache,
            _logger,
            ct
        );

        return connectorVersions;
    }

    /// <inheritdoc />
    public async Task<NuGetVersion> GetLatestVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        var versions = await GetVersion(id, ct);
        return versions.Last(v => v.IsPrerelease == prerelease);
    }

    /// <inheritdoc />
    public async Task<PackageArchiveReader> GetConnectorPackage(
        string id,
        NuGetVersion version,
        CancellationToken ct = default)
    {
        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();

        var ms = new MemoryStream();

        await resource.CopyNupkgToStreamAsync(id, version, ms, cache, _logger, ct);

        var packageReader = new PackageArchiveReader(ms);

        return packageReader;
    }

    internal virtual SourceRepository GetSourceRepository(PackageSource? source) =>
        Repository.Factory.GetCoreV3(source);

    internal virtual SourceRepository GetSourceRepository(string? source) =>
        Repository.Factory.GetCoreV3(source);

    private async Task<T> GetPrivateResource<T>(CancellationToken ct)
        where T : class, INuGetResource
    {
        SourceRepository repository;

        if (!string.IsNullOrEmpty(_settings.RegistryUser)
         || !string.IsNullOrEmpty(_settings.RegistryToken))
        {
            var source = new PackageSource(_settings.Uri)
            {
                Credentials = new PackageSourceCredential(
                    source: _settings.Uri,
                    username: _settings.RegistryUser,
                    passwordText: _settings.RegistryToken,
                    isPasswordClearText: true,
                    validAuthenticationTypesText: null
                )
            };

            repository = GetSourceRepository(source);
        }
        else
        {
            repository = GetSourceRepository(_settings.Uri);
        }

        return await repository.GetResourceAsync<T>(ct);
    }
}

}
