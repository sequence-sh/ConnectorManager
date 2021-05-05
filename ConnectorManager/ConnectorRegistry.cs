using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
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
    public async Task<ICollection<ConnectorPackage>> Find(
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

        return results.Select(
                p => new ConnectorPackage(p.Identity.Id, p.Identity.Version.ToNormalizedString())
            )
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> Exists(
        string id,
        string? version = null,
        CancellationToken ct = default)
    {
        var nugetVersion = await GetNuGetVersion(id, version, true, ct);
        var resource     = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache        = new SourceCacheContext();
        return await resource.DoesPackageExistAsync(id, nugetVersion, cache, _logger, ct);
    }

    /// <inheritdoc />
    public async Task<ICollection<string>> GetVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();
        var versions = await resource.GetAllVersionsAsync(id, cache, _logger, ct);

        return versions.Where(v => v.IsPrerelease == prerelease)
            .Select(v => v.ToNormalizedString())
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<string> GetLatestVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        var versions = await GetVersion(id, prerelease, ct);
        return versions.Last();
    }

    /// <inheritdoc />
    public async Task<ConnectorPackage> GetConnectorPackage(
        string id,
        string? version,
        CancellationToken ct = default)
    {
        var nugetVersion = await GetNuGetVersion(id, version, true, ct);

        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();

        var ms = new MemoryStream();

        await resource.CopyNupkgToStreamAsync(id, nugetVersion, ms, cache, _logger, ct);

        return new ConnectorPackage(id, nugetVersion.ToNormalizedString(), ms);
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

    internal async Task<NuGetVersion> GetNuGetVersion(
        string id,
        string? version = null,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        var check = true;

        if (string.IsNullOrEmpty(version))
        {
            version = await GetLatestVersion(id, prerelease, ct);
            check   = false;
        }

        var nuGetVersion = NuGetVersion.Parse(version);

        if (nuGetVersion == null)
            throw new ArgumentException(
                $"Could not parse version string '{version}'",
                nameof(version)
            );

        if (check && !await Exists(id, nuGetVersion.ToNormalizedString(), ct))
            throw new VersionNotFoundException(
                $"Could not find version '{nuGetVersion.ToNormalizedString()}' for connector {id}"
            );

        return nuGetVersion;
    }
}

}
