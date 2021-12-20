using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.ConnectorManagement;

/// <inheritdoc />
public class ConnectorManager : IConnectorManager
{
    private readonly ILogger<ConnectorManager> _logger;
    private readonly ConnectorManagerSettings _settings;
    private readonly IConnectorRegistry _registry;
    private readonly IConnectorConfiguration _configuration;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Create connector manager
    /// </summary>
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

    /// <summary>
    /// Create a new Connector Manager with connector settings
    /// </summary>
    public static async Task<ConnectorManager> CreateAndPopulate(
        ILogger<ConnectorManager> logger,
        ConnectorManagerSettings settings,
        IConnectorRegistry connectorRegistry,
        IFileSystem fileSystem,
        Dictionary<string, ConnectorSettings>? settingsDictionary)
    {
        if (settingsDictionary is null || settingsDictionary.Count == 0)
        {
            //load latest connectors from repository
            var manager1 = new ConnectorManager(
                logger,
                settings,
                connectorRegistry,
                new ConnectorConfiguration(),
                fileSystem
            );

            var found = await manager1.Find(); //Find all connectors

            settingsDictionary = found.ToDictionary(
                x => x.Id,
                x => new ConnectorSettings()
                {
                    Enable = true, Id = x.Id, Version = GetBestVersion(x.Version)
                }
            );
        }

        var connectorManager = new ConnectorManager(
            logger,
            settings,
            connectorRegistry,
            new ConnectorConfiguration(settingsDictionary),
            fileSystem
        );

        return connectorManager;

        static string GetBestVersion(string latestVersionString)
        {
            try
            {
                var latestVersion = Version.Parse(latestVersionString);

                var thisVersion = Assembly.GetEntryAssembly()!.GetName().Version!;

                if (latestVersion.Major > thisVersion.Major
                 || latestVersion.Minor > thisVersion.Minor)
                    return thisVersion.ToString();

                return latestVersionString;
            }
            catch (Exception)
            {
                //In case something goes wrong
                return latestVersionString;
            }
        }
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
        var versionCheck = await CheckVersion(id, version, prerelease, ct);

        if (versionCheck == null || (version != null && !versionCheck.IsValid))
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
                    "Connector configuration '{configuration}' already exists. Use --force to overwrite.",
                    name
                );

                return;
            }
        }

        var installPath = GetInstallPath(id, versionCheck.Version);

        var package = await InstallConnector(id, versionCheck.Version, installPath, force, ct);

        if (package == null)
            return;

        await _configuration.AddAsync(
            name,
            new ConnectorSettings { Id = package.Id, Version = package.Version, Enable = true },
            ct
        );

        _logger.LogDebug(
            "Successfully installed connector '{id}' - '{version}'.",
            id,
            versionCheck.Version
        );
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
            _logger.LogInformation(
                "Connector configuration '{configuration}' already has version '{version}'.",
                name,
                version
            );

            return;
        }

        var versionCheck = await CheckVersion(cs.Id, version, prerelease, ct);

        if (versionCheck == null || !versionCheck.IsValid || versionCheck.IsLatest)
            return;

        if (version == null && cs.Version.Equals(versionCheck.Version))
        {
            _logger.LogInformation(
                "Connector configuration '{configuration}' already has the latest version '{version}' installed.",
                name,
                versionCheck.Version
            );

            return;
        }

        var installPath = GetInstallPath(cs.Id, versionCheck.Version);

        var package = await InstallConnector(cs.Id, versionCheck.Version, installPath, false, ct);

        if (package == null)
            return;

        cs.Version = versionCheck.Version;

        _configuration[name] = cs;

        _logger.LogInformation(
            "Connector configuration '{configuration}' successfully updated to '{version}'.",
            name,
            versionCheck.Version
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

            Assembly? plugin;

            try
            {
                plugin = LoadPlugin(dllPath, _logger);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "Failed to load connector configuration '{configuration}' from '{installPath}'. Exception: {exception}",
                    key,
                    dir,
                    e
                );

                continue;
            }

            yield return (key, new ConnectorData(settings, plugin));
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

    /// <summary>
    /// Load connector from the dllPath.
    /// </summary>
    protected internal virtual Assembly LoadPlugin(string dllPath, ILogger logger) =>
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

    private record VersionCheck(string Version, string Latest, bool IsValid, bool IsLatest);

    private async Task<VersionCheck?> CheckVersion(
        string id,
        string? version,
        bool prerelease,
        CancellationToken ct)
    {
        var allVersions = await _registry.GetVersion(id, prerelease, ct);

        if (allVersions.Count == 0)
        {
            _logger.LogError("Could not find connector '{connectorId}' in the registry.", id);
            return null;
        }

        var latest = allVersions.Last();

        if (version == null)
            return new VersionCheck(latest, latest, true, false);

        var isValid = allVersions.Contains(version, StringComparer.OrdinalIgnoreCase);

        if (!isValid)
            _logger.LogError(
                "Could not find connector '{connectorId}' version '{version}' in the registry.",
                id,
                version
            );

        var isLatest = version.Equals(latest, StringComparison.OrdinalIgnoreCase);

        if (isLatest)
            _logger.LogInformation(
                "Version '{version}' is the latest available for '{connectorId}'.",
                version,
                id
            );

        return new VersionCheck(version, latest, isValid, isLatest);
    }
}
