using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
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
    private readonly IConnectorRegistrySettings _settings;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="connectorManagerSettings"></param>
    public ConnectorRegistry(
        Microsoft.Extensions.Logging.ILogger<ConnectorRegistry> logger,
        IConnectorRegistrySettings connectorManagerSettings)
    {
        _logger   = new LoggerBridge<ConnectorRegistry>(logger);
        _settings = connectorManagerSettings;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IPackageSearchMetadata>> Find(
        string search,
        CancellationToken ct,
        bool prerelease = false)
    {
        var resource = await GetPrivateResource<PackageSearchResource>(ct);

        var results = await resource.SearchAsync(
            search,
            new SearchFilter(prerelease),
            skip: 0,
            take: 100,
            _logger,
            ct
        );

        return results;
    }

    /// <inheritdoc />
    public async Task<bool> Exists(string id, NuGetVersion version, CancellationToken ct)
    {
        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();
        return await resource.DoesPackageExistAsync(id, version, cache, _logger, ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IPackageSearchMetadata>> GetMetadata(
        string id,
        CancellationToken ct,
        bool prerelease = false)
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
    public async Task<IEnumerable<NuGetVersion>> GetVersion(string id, CancellationToken ct)
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
        CancellationToken ct,
        bool prerelease = false)
    {
        var versions = await GetVersion(id, ct);
        return versions.Last(v => v.IsPrerelease == prerelease);
    }

    /// <inheritdoc />
    public async Task<PackageIdentity?> Install(
        string id,
        NuGetVersion version,
        string path,
        CancellationToken ct,
        bool force = false)
    {
        if (Directory.Exists(path))
        {
            if (force)
            {
                Directory.Delete(path, true);
            }
            else
            {
                _logger.LogVerbose($"{path} already exists");
                return null;
            }
        }

        var connectorDir = Directory.CreateDirectory(path);

        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();

        await using var ms = new MemoryStream();

        await resource.CopyNupkgToStreamAsync(id, version, ms, cache, _logger, ct);

        using var packageReader = new PackageArchiveReader(ms);

        var files = await packageReader.GetPackageFilesAsync(PackageSaveMode.Files, ct);

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            var entry       = packageReader.GetEntry(file);
            var extractPath = Path.Combine(connectorDir.FullName, entry.Name);
            entry.ExtractToFile(extractPath);
        }

        return await packageReader.GetIdentityAsync(ct);
    }

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

            repository = Repository.Factory.GetCoreV3(source);
        }
        else
        {
            repository = Repository.Factory.GetCoreV3(_settings.Uri);
        }

        return await repository.GetResourceAsync<T>(ct);
    }
}

}
