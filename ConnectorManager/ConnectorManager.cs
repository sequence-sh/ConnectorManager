using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.ConnectorManagement
{

/// <inheritdoc />
public class ConnectorManager : IConnectorManager
{
    private readonly ILogger<ConnectorManager> _logger;
    private readonly ConnectorManagerSettings _settings;
    private readonly IConnectorRegistry _registry;
    private readonly IConnectorConfiguration _configuration;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="settings"></param>
    /// <param name="registry"></param>
    /// <param name="configuration"></param>
    public ConnectorManager(
        ILogger<ConnectorManager> logger,
        ConnectorManagerSettings settings,
        IConnectorRegistry registry,
        IConnectorConfiguration configuration)
    {
        _logger        = logger;
        _settings      = settings;
        _registry      = registry;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task Add(
        string id,
        string? name,
        string? version,
        bool prerelease = false,
        bool force = false,
        CancellationToken ct = default)
    {
        var nuGetVersion = await GetNuGetVersion(id, version, prerelease, ct);

        var installPath = GetInstallPath(id, nuGetVersion.ToNormalizedString());

        if (_configuration.ContainsVersionString(id, nuGetVersion.ToNormalizedString()) && !force)
        {
            _logger.LogInformation(
                $"Connector {id} {nuGetVersion.ToNormalizedString()} is already installed"
            );

            return;
        }

        var package = await InstallConnector(id, nuGetVersion, installPath, force, ct);

        if (package == null)
            throw new Exception($"Could not install connector to {installPath}");

        if (_configuration.ContainsId(id))
        {
            foreach (var config in _configuration.Settings.Where(
                c => c.Enable && c.Id.Equals(id, StringComparison.Ordinal)
            ))
                config.Enable = false;
        }

        if (_configuration.Contains(id))
        {
            var cs = _configuration[id];

            cs.Version = nuGetVersion.ToNormalizedString();
            cs.Enable  = true;

            _configuration[id] = cs;
        }
        else
        {
            await _configuration.AddAsync(
                id,
                new ConnectorSettings
                {
                    Id = id, Version = nuGetVersion.ToNormalizedString(), Enable = true
                },
                ct
            );
        }
    }

    /// <inheritdoc />
    public async Task Update(
        string name,
        string? version,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        if (!_configuration.Contains(name))
        {
            _logger.LogInformation($"Connector not found: {name}");
            return;
        }

        var cs = _configuration[name];

        var nuGetVersion = await GetNuGetVersion(name, version, prerelease, ct);

        if (cs.Version.Equals(nuGetVersion.ToNormalizedString()))
        {
            _logger.LogInformation(
                $"Latest version already installed: {name} {nuGetVersion.ToNormalizedString()}"
            );

            return;
        }

        var installPath = GetInstallPath(name, nuGetVersion.ToNormalizedString());

        var package = await _registry.GetConnectorPackage(name, nuGetVersion, ct);

        if (package == null)
            throw new Exception($"Could not install connector to {installPath}");

        cs.Version = nuGetVersion.ToNormalizedString();

        _configuration[name] = cs;
    }

    /// <inheritdoc />
    public async Task Remove(string name, CancellationToken ct = default)
    {
        if (_configuration.TryGetSettings(name, out var connector))
        {
            var removePath = GetInstallPath(name, connector.Version);

            try
            {
                Directory.Delete(removePath, true);
            }
            catch (DirectoryNotFoundException)
            {
                _logger.LogWarning($"Directory not found {removePath}");
            }

            await _configuration.RemoveAsync(name, ct);
        }
        else
        {
            _logger.LogError($"Could not find connector {name}");
        }
    }

    /// <inheritdoc />
    public void List(string? nameFilter)
    {
        if (_configuration.Count <= 0)
            return;

        var maxNameLen = _configuration.Max(c => c.Key.Length) + 2;
        var maxIdLen   = _configuration.Max(c => c.Value.Id.Length) + 2;
        var maxVerLen  = _configuration.Max(c => c.Value.Version.Length) + 2;

        if (maxVerLen < 7)
            maxVerLen = 7;

        var outputFormat = $" {{0,-{maxNameLen}}} {{1,-{maxIdLen}}} {{2,-{maxVerLen}}} {{3,-9}}";

        Console.WriteLine(outputFormat, "Name", "Id", "Version", "Enabled");
        Console.WriteLine(new string('-', maxNameLen + maxIdLen + maxVerLen + 11));

        var configs = nameFilter == null
            ? _configuration
            : _configuration.Where(c => Regex.IsMatch(c.Key, nameFilter));

        foreach (var (k, v) in configs)
            Console.WriteLine(outputFormat, k, v.Id, v.Version, v.Enable);
    }

    /// <inheritdoc />
    public async Task Find(string? search, bool prerelease = false, CancellationToken ct = default)
    {
        var found      = await _registry.Find(search ?? string.Empty, prerelease, ct);
        var connectors = found.ToList();

        if (connectors.Count <= 0)
            return;

        var maxIdLen  = _configuration.Max(c => c.Value.Id.Length) + 2;
        var maxVerLen = _configuration.Max(c => c.Value.Version.Length) + 2;

        if (maxVerLen < 7)
            maxVerLen = 7;

        var outputFormat = $" {{0,-{maxIdLen}}} {{1,-{maxVerLen}}}";

        Console.WriteLine(outputFormat, "Id", "Version");
        Console.WriteLine(new string('-', maxIdLen + maxVerLen + 2));

        foreach (var c in connectors)
            Console.WriteLine(outputFormat, c.Identity.Id, c.Identity.Version);
    }

    private async Task<PackageIdentity?> InstallConnector(
        string id,
        NuGetVersion version,
        string path,
        bool force = false,
        CancellationToken ct = default)
    {
        if (Directory.Exists(path))
        {
            if (force)
            {
                Directory.Delete(path, true);
            }
            else
            {
                _logger.LogDebug($"{path} already exists");
                return null;
            }
        }

        var connectorDir = Directory.CreateDirectory(path);

        using var packageReader = await _registry.GetConnectorPackage(id, version, ct);

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

    private string GetInstallPath(string id, string version) =>
        Path.Combine(_settings.ConnectorPath, id, version);

    private async Task<NuGetVersion> GetNuGetVersion(
        string id,
        string? version,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        NuGetVersion nuGetVersion;

        if (version == null)
        {
            nuGetVersion = await _registry.GetLatestVersion(id, prerelease, ct);
        }
        else
        {
            nuGetVersion = NuGetVersion.Parse(version);

            if (!await _registry.Exists(id, nuGetVersion, ct))
                throw new VersionNotFoundException(
                    $"Connector does not exist in registry: {id} {nuGetVersion.ToNormalizedString()}"
                );
        }

        return nuGetVersion;
    }
}

}
