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
        _logger = connectorRegistrySettings.EnableNuGetLog
            ? new LoggerBridge<ConnectorRegistry>(logger)
            : new NullLogger();

        _settings = connectorRegistrySettings;
    }

    /// <inheritdoc />
    public async Task<ICollection<ConnectorMetadata>> Find(
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
                p => new ConnectorMetadata(p.Identity.Id, p.Identity.Version.ToNormalizedString())
            )
            .ToArray();
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
        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();
        var versions = await resource.GetAllVersionsAsync(id, cache, _logger, ct);

        return versions.Where(v => prerelease || !v.IsPrerelease)
            .Select(v => v.ToNormalizedString())
            .ToArray();
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

        var resource = await GetPrivateResource<FindPackageByIdResource>(ct);
        var cache    = new SourceCacheContext();

        var ms = new MemoryStream();

        await resource.CopyNupkgToStreamAsync(id, nugetVersion, ms, cache, _logger, ct);

        if (ms.Length == 0)
            throw new ArgumentException($"Can't find connector {id} ({version})");

        var packageReader = new PackageArchiveReader(ms);

        var packageIdentity = await packageReader.GetIdentityAsync(ct);

        return new ConnectorPackage(
            new ConnectorMetadata(packageIdentity.Id, packageIdentity.Version.ToNormalizedString()),
            packageReader
        );
    }

    private async Task<T> GetPrivateResource<T>(CancellationToken ct)
        where T : class, INuGetResource
    {
        SourceRepository repository;

        if (!string.IsNullOrEmpty(_settings.User)
         || !string.IsNullOrEmpty(_settings.Token))
        {
            var source = new PackageSource(_settings.Uri)
            {
                Credentials = new PackageSourceCredential(
                    source: _settings.Uri,
                    username: _settings.User,
                    passwordText: _settings.Token,
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
