using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoreLinq;
using NuGet.Versioning;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR
{

/// <inheritdoc />
public class ConnectorManager : IConnectorManager
{
    private readonly ILogger<ConnectorManager> _logger;
    private readonly IConnectorManagerSettings _settings;
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
        IConnectorManagerSettings settings,
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
        string? version,
        CancellationToken ct,
        bool prerelease = false,
        bool force = false)
    {
        var nuGetVersion = await GetNuGetVersion(id, version, ct, prerelease);

        var installPath = GetInstallPath(id, nuGetVersion.ToNormalizedString());

        if (_configuration.ContainsVersionString(id, nuGetVersion.ToNormalizedString()) && !force)
        {
            _logger.LogInformation(
                $"Connector {id} {nuGetVersion.ToNormalizedString()} is already installed"
            );

            return;
        }

        var package = await _registry.Install(id, nuGetVersion, installPath, ct, force);

        if (package == null)
            throw new Exception($"Could not install connector to {installPath}");

        if (_configuration.ContainsId(id))
            _configuration.Connectors
                .Where(c => c.Enable && c.Id.Equals(id, StringComparison.Ordinal))
                .ForEach(c => c.Enable = false);

        if (_configuration.Contains(id))
        {
            var cs = _configuration[id];

            cs.Version = nuGetVersion.ToNormalizedString();
            cs.Enable  = true;

            _configuration[id] = cs;
        }
        else
        {
            _configuration.Add(
                id,
                new ConnectorSettings
                {
                    Id = id, Version = nuGetVersion.ToNormalizedString(), Enable = true
                }
            );
        }
    }

    /// <inheritdoc />
    public async Task Update(
        string id,
        string? version,
        CancellationToken ct,
        bool prerelease = false)
    {
        if (!_configuration.Contains(id))
        {
            _logger.LogInformation($"Connector not found: {id}");
            return;
        }

        var cs = _configuration[id];

        var nuGetVersion = await GetNuGetVersion(id, version, ct, prerelease);

        if (cs.Version.Equals(nuGetVersion.ToNormalizedString()))
        {
            _logger.LogInformation(
                $"Latest version already installed: {id} {nuGetVersion.ToNormalizedString()}"
            );

            return;
        }

        var installPath = GetInstallPath(id, nuGetVersion.ToNormalizedString());

        var package = await _registry.Install(id, nuGetVersion, installPath, ct);

        if (package == null)
            throw new Exception($"Could not install connector to {installPath}");

        cs.Version = nuGetVersion.ToNormalizedString();

        _configuration[id] = cs;
    }

    /// <inheritdoc />
    public void Remove(string id)
    {
        if (_configuration.TryGetValue(id, out var connector))
        {
            var removePath = GetInstallPath(id, connector.Version);

            try
            {
                Directory.Delete(removePath, true);
            }
            catch (DirectoryNotFoundException)
            {
                _logger.LogWarning($"Directory not found {removePath}");
            }

            _configuration.Remove(id);
        }
        else
        {
            _logger.LogError($"Could not find connector {id}");
        }
    }

    /// <inheritdoc />
    public void List(string? filter)
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

        var configs = filter == null
            ? _configuration
            : _configuration.Where(c => Regex.IsMatch(c.Key, filter));

        foreach (var (k, v) in configs)
            Console.WriteLine(outputFormat, k, v.Id, v.Version, v.Enable);
    }

    /// <inheritdoc />
    public async Task Find(string? search, CancellationToken ct, bool prerelease = false)
    {
        var found      = await _registry.Find(search ?? string.Empty, ct, prerelease);
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

    private string GetInstallPath(string id, string version) =>
        Path.Combine(_settings.ConnectorPath, id, version);

    private async Task<NuGetVersion> GetNuGetVersion(
        string id,
        string? version,
        CancellationToken ct,
        bool prerelease = false)
    {
        NuGetVersion nuGetVersion;

        if (version == null)
        {
            nuGetVersion = await _registry.GetLatestVersion(id, ct, prerelease);
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
