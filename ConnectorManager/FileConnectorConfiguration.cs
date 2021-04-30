using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR
{

/// <summary>
/// A ConnectorConfiguration that uses a JSON file as the backing store.
/// </summary>
public class FileConnectorConfiguration : IConnectorConfiguration
{
    private readonly string _path;
    private readonly Dictionary<string, ConnectorSettings> _connectors;

    /// <summary>
    /// Create a new ConnectorConfiguration using a json file as a store.
    /// </summary>
    /// <param name="path">Path to the connector configuration file.</param>
    /// <exception cref="FileNotFoundException">If the configuration file does not exist.</exception>
    public FileConnectorConfiguration(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Connector configuration file not found.", path);

        var text = File.ReadAllText(path);

        _path = path;

        _connectors = JsonConvert.DeserializeObject<Dictionary<string, ConnectorSettings>>(
            text,
            EntityJsonConverter.Instance
        ) ?? new Dictionary<string, ConnectorSettings>();
    }

    private void SaveSettings()
    {
        var output = JsonConvert.SerializeObject(_connectors, EntityJsonConverter.Instance);
        File.WriteAllText(_path, output);
    }

    /// <inheritdoc />
    public ICollection<ConnectorSettings> Connectors => _connectors.Values;

    /// <inheritdoc />
    public int Count => _connectors.Count;

    /// <inheritdoc />
    public void Add(string name, ConnectorSettings settings)
    {
        _connectors.Add(name, settings);
        SaveSettings();
    }

    /// <inheritdoc />
    public bool Remove(string name)
    {
        var success = _connectors.Remove(name);

        if (success)
            SaveSettings();

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
    public bool TryGetValue(string name, out ConnectorSettings settings) =>
        _connectors.TryGetValue(name, out settings!);

    /// <inheritdoc />
    public ConnectorSettings this[string name]
    {
        get => _connectors[name];
        set
        {
            _connectors[name] = value;
            SaveSettings();
        }
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, ConnectorSettings>> GetEnumerator() =>
        _connectors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

}
