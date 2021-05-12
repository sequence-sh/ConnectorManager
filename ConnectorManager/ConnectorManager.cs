using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Connectors;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

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
        (version, _) = await CheckVersion(id, version, prerelease, ct);

        if (version == null)
            return;

        if (string.IsNullOrEmpty(name))
            name = id;

        if (_configuration.Contains(name))
        {
            if (force)
            {
                await _configuration.RemoveAsync(name, ct);
                _logger.LogDebug("Removed '{configuration}' from connector configuration.", name);
            }
            else
            {
                _logger.LogError(
                    "Connector configuration '{configuration}' already exists. Use --force to overwrite or update.",
                    name
                );

                return;
            }
        }

        var installPath = GetInstallPath(id, version);

        var package = await InstallConnector(id, version, installPath, force, ct);

        if (package == null)
            return;

        await _configuration.AddAsync(
            name,
            new ConnectorSettings { Id = package.Id, Version = package.Version, Enable = true },
            ct
        );

        _logger.LogDebug("Successfully installed connector '{id}' - '{version}'.", id, version);
    }

    /// <inheritdoc />
    public async Task Update(
        string name,
        string? version = null,
        bool prerelease = false,
        CancellationToken ct = default)
    {
        if (!_configuration.TryGetSettings(name, out var cs))
        {
            _logger.LogError(
                "Connector configuration '{configuration}' does not exist. To install, use add.",
                name
            );

            return;
        }

        if (version != null && cs.Version.Equals(version))
        {
            _logger.LogError(
                "Connector configuration '{configuration}' already has version '{version}'.",
                name,
                version
            );

            return;
        }

        string? latest;

        (version, latest) = await CheckVersion(cs.Id, version, prerelease, ct);

        if (version == null)
            return;

        if (version.Equals(latest, StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "Connector configuration '{configuration}' already has the latest version '{version}' installed.",
                name,
                version
            );

            return;
        }

        var installPath = GetInstallPath(cs.Id, version);

        var package = await InstallConnector(cs.Id, version, installPath, false, ct);

        if (package == null)
            return;

        cs.Version = version;

        _configuration[name] = cs;

        _logger.LogInformation(
            "Connector configuration '{configuration}' successfully updated to '{version}'.",
            name,
            version
        );
    }

    /// <inheritdoc />
    public async Task Remove(
        string name,
        bool configurationOnly = false,
        CancellationToken ct = default)
    {
        if (_configuration.TryGetSettings(name, out var cs))
        {
            if (!configurationOnly)
            {
                var removePath = GetInstallPath(cs.Id, cs.Version);

                try
                {
                    _fileSystem.Directory.Delete(removePath, true);
                }
                catch (DirectoryNotFoundException)
                {
                    _logger.LogWarning(
                        "Connector directory '{connectorDirectory}' not found.",
                        removePath
                    );
                }
            }

            await _configuration.RemoveAsync(name, ct);

            _logger.LogDebug("Connector configuration '{configuration}' removed.", name);
        }
        else
        {
            _logger.LogError("Connector configuration '{configuration}' not found.", name);
        }
    }

    /// <inheritdoc />
    public IEnumerable<(string name, ConnectorData data)> List(string? nameFilter = null)
    {
        if (_configuration.Count <= 0)
            yield break;

        var configs = nameFilter == null
            ? _configuration
            : _configuration.Where(c => Regex.IsMatch(c.Key, nameFilter));

        foreach (var (key, settings) in configs)
        {
            var dir     = GetInstallPath(settings.Id, settings.Version);
            var dllPath = _fileSystem.Path.Combine(dir, $"{settings.Id}.dll");

            var loadResult = LoadPlugin(dllPath, _logger);

            if (loadResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to load connector configuration '{configuration}' from '{installPath}'.",
                    key,
                    dir
                );

                continue;
            }

            yield return (key, new ConnectorData(settings, loadResult.Value));
        }
    }

    /// <inheritdoc />
    public async Task<bool> Verify(CancellationToken ct = default)
    {
        var success = true;

        foreach (var (key, settings) in _configuration)
        {
            _logger.LogDebug("Checking connector configuration '{name}'.", key);

            var dir     = GetInstallPath(settings.Id, settings.Version);
            var dllPath = _fileSystem.Path.Combine(dir, $"{settings.Id}.dll");

            if (_fileSystem.Directory.Exists(dir))
            {
                _logger.LogDebug(
                    "Verified connector '{connector}' install path exists: {directory}",
                    settings.Id,
                    dir
                );

                if (_fileSystem.File.Exists(dllPath))
                {
                    _logger.LogDebug(
                        "Verified connector '{connector}' dll exists: {dllPath}",
                        settings.Id,
                        dllPath
                    );
                }
                else
                {
                    _logger.LogError(
                        "Configuration '{configuration}' connector dll missing: {dllPath}",
                        key,
                        dllPath
                    );

                    success = false;
                }

                continue;
            }

            if (_settings.AutoDownload)
            {
                await InstallConnector(settings.Id, settings.Version, dir, false, ct);
            }
            else
            {
                _logger.LogError(
                    "Configuration '{configuration}' installation path missing: {path}",
                    key,
                    dir
                );

                success = false;
            }
        }

        return success;
    }

    /// <inheritdoc />
    public async Task<ICollection<ConnectorMetadata>> Find(
        string? search = null,
        bool prerelease = false,
        CancellationToken ct = default) =>
        (await _registry.Find(search ?? string.Empty, prerelease, ct)).ToList();

    internal virtual Result<Assembly, IErrorBuilder> LoadPlugin(string dllPath, ILogger logger) =>
        PluginLoadContext.LoadPlugin(dllPath, _logger);

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
                    "Connector directory '{connectorDir}' already exists. Use --force to overwrite.",
                    path
                );

                return null;
            }
        }

        _logger.LogDebug(
            "Installing connector {connector} - {version} to: {installDir}",
            id,
            version,
            path
        );

        using var package = await _registry.GetConnectorPackage(id, version, ct);

        _fileSystem.Directory.CreateDirectory(path);

        await package.Extract(_fileSystem, path, ct);

        _logger.LogDebug(
            "Successfully downloaded and extracted '{id}' - '{version}'.",
            id,
            version
        );

        return package.Metadata;
    }

    private async Task<(string? version, string? latest)> CheckVersion(
        string id,
        string? version,
        bool prerelease,
        CancellationToken ct)
    {
        var allVersions = await _registry.GetVersion(id, prerelease, ct);

        if (allVersions.Count == 0)
        {
            _logger.LogError("Could not find connector '{connectorId}' in the registry.", id);
            return (null, null);
        }

        var latest = allVersions.Last();

        if (version == null)
            return (latest, latest);

        if (allVersions.Contains(version))
            return (version, latest);

        _logger.LogError(
            "Could not find connector '{connectorId}' version '{version}' in the registry.",
            id,
            version
        );

        return (null, latest);
    }
}

}
