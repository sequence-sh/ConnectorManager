using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// A ConnectorConfiguration that uses a JSON file as the backing store.
/// </summary>
public class FileConnectorConfiguration : IConnectorConfiguration
{
    private readonly string _path;
    private readonly Dictionary<string, ConnectorSettings> _connectors;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Create a new ConnectorConfiguration using a JSON file.
    /// </summary>
    public static async Task<IConnectorConfiguration> FromJson(
        string configurationPath,
        IFileSystem fileSystem)
    {
        if (!fileSystem.File.Exists(configurationPath))
            throw new FileNotFoundException(
                "Connector configuration file not found.",
                configurationPath
            );

        var text = await fileSystem.File.ReadAllTextAsync(configurationPath);

        var connectors = JsonConvert.DeserializeObject<Dictionary<string, ConnectorSettings>>(
            text,
            EntityJsonConverter.Instance
        ) ?? new Dictionary<string, ConnectorSettings>();

        return new FileConnectorConfiguration(configurationPath, connectors, fileSystem);
    }

    private FileConnectorConfiguration(
        string path,
        Dictionary<string, ConnectorSettings> connectors,
        IFileSystem fileSystem)
    {
        _path       = path;
        _connectors = connectors;
        _fileSystem = fileSystem;
    }

    private async Task SaveSettings(CancellationToken ct)
    {
        var output = JsonConvert.SerializeObject(_connectors, EntityJsonConverter.Instance);
        await _fileSystem.File.WriteAllTextAsync(_path, output, ct);
    }

    /// <inheritdoc />
    public ICollection<ConnectorSettings> Connectors => _connectors.Values;

    /// <inheritdoc />
    public int Count => _connectors.Count;

    /// <inheritdoc />
    public void Add(string name, ConnectorSettings settings) =>
        AddAsync(name, settings, CancellationToken.None).Wait();

    /// <inheritdoc />
    public async Task AddAsync(string name, ConnectorSettings settings, CancellationToken ct)
    {
        _connectors.Add(name, settings);
        await SaveSettings(ct);
    }

    /// <inheritdoc />
    public bool Remove(string name) => RemoveAsync(name, CancellationToken.None).Result;

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string name, CancellationToken ct)
    {
        var success = _connectors.Remove(name);

        if (success)
            await SaveSettings(ct);

        return success;
    }

    /// <inheritdoc />
    public bool Contains(string name) => _connectors.ContainsKey(name);

    /// <inheritdoc />
    public bool ContainsId(string id) =>
        _connectors.Values.Any(c => c.Id.Equals(id, StringComparison.Ordinal));

    /// <inheritdoc />
    public bool ContainsVersionString(string id, string version) => _connectors.Values.Any(
        c => c.VersionString().Equals($"{id} {version}", StringComparison.Ordinal)
    );

    /// <inheritdoc />
    public bool TryGetSettings(string name, out ConnectorSettings settings) =>
        _connectors.TryGetValue(name, out settings!);

    /// <inheritdoc />
    public bool TryGetSettingsById(string id, out ConnectorSettings[] settings)
    {
        settings = _connectors.Values.Where(c => c.Id.Equals(id, StringComparison.Ordinal))
            .ToArray();

        return settings.Length > 0;
    }

    /// <inheritdoc />
    public ConnectorSettings this[string name]
    {
        get => _connectors[name];
        set
        {
            _connectors[name] = value;
            SaveSettings(CancellationToken.None).Wait();
        }
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, ConnectorSettings>> GetEnumerator() =>
        _connectors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

}
