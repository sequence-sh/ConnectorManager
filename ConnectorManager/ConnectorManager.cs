using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="settings"></param>
    /// <param name="registry"></param>
    /// <param name="configuration"></param>
    /// <param name="fileSystem"></param>
    public ConnectorManager(
        ILogger<ConnectorManager> logger,
        ConnectorManagerSettings settings,
        IConnectorRegistry registry,
        IConnectorConfiguration configuration,
        IFileSystem fileSystem)
    {
        _logger        = logger;
        _settings      = settings;
        _registry      = registry;
        _configuration = configuration;
        _fileSystem    = fileSystem;
    }

    /// <inheritdoc />
    public async Task Add(
        string id,
        string? name = null,
        string? version = null,
        bool prerelease = false,
        bool force = false,
        CancellationToken ct = default)
    {
        var allVersions = await _registry.GetVersion(id, prerelease, ct);

        if (allVersions.Count == 0)
        {
            _logger.LogError($"Could not find connector {id} in the registry.");
            return;
        }

        if (version == null)
            version = allVersions.Last();
        else if (!allVersions.Contains(version))
        {
            _logger.LogError($"Could not find connector {id} version {version} in the registry.");
            return;
        }

        if (string.IsNullOrEmpty(name))
            name = id;

        if (_configuration.Contains(name))
        {
            if (force)
            {
                await _configuration.RemoveAsync(name, ct);
                _logger.LogDebug($"Removed {name} from connector configuration.");
            }
            else
            {
                _logger.LogError(
                    $"Connector configuration already exists {name}. Use --force to overwrite or update."
                );

                return;
            }
        }

        var installPath = GetInstallPath(id, version);

        var package = await InstallConnector(id, version, installPath, force, ct);

        if (package == null)
            return;

        await _configuration.AddAsync(
            id,
            new ConnectorSettings { Id = package.Id, Version = package.Version, Enable = true },
            ct
        );

        _logger.LogInformation($"Successfully installed connector {id} ({version}).");
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

        //var nuGetVersion = await GetNuGetVersion(name, version, prerelease, ct);

        var latest = "";

        if (cs.Version.Equals(latest))
        {
            _logger.LogInformation($"Latest version already installed: {name} {latest}");

            return;
        }

        var installPath = GetInstallPath(name, latest);

        var package = await _registry.GetConnectorPackage(name, latest, ct);

        if (package == null)
            throw new Exception($"Could not install connector to {installPath}");

        cs.Version = latest;

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
            Console.WriteLine(outputFormat, c.Id, c.Version);
    }

    private string GetInstallPath(string id, string version) =>
        _fileSystem.Path.Combine(_settings.ConnectorPath, id, version);

    private async Task<ConnectorMetadata?> InstallConnector(
        string id,
        string version,
        string path,
        bool force = false,
        CancellationToken ct = default)
    {
        if (_fileSystem.Directory.Exists(path))
        {
            if (force)
                _fileSystem.Directory.Delete(path, true);
            else
            {
                _logger.LogError(
                    $"Connector directory {path} already exists. Use --force to overwrite."
                );

                return null;
            }
        }

        _fileSystem.Directory.CreateDirectory(path);

        using var package = await _registry.GetConnectorPackage(id, version, ct);

        await package.Extract(_fileSystem, path, ct);

        return package.Metadata;
    }
}

}
