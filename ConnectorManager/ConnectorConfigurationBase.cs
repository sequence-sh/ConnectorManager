using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.ConnectorManagement;

/// <inheritdoc />
public abstract class ConnectorConfigurationBase : IConnectorConfiguration
{
    /// <summary>
    /// Backing dictionary
    /// </summary>
    protected abstract Dictionary<string, ConnectorSettings> Connectors { get; }

    /// <inheritdoc />
    public ICollection<string> Keys => Connectors.Keys;

    /// <inheritdoc />
    public ICollection<ConnectorSettings> Settings => Connectors.Values;

    /// <inheritdoc />
    public int Count => Connectors.Count;

    /// <inheritdoc />
    public abstract Task AddAsync(
        string name,
        ConnectorSettings settings,
        CancellationToken ct = default);

    /// <inheritdoc />
    public abstract Task<bool> RemoveAsync(string name, CancellationToken ct = default);

    /// <inheritdoc />
    public bool Contains(string name) => Connectors.ContainsKey(name);

    /// <inheritdoc />
    public bool ContainsId(string id) =>
        Connectors.Values.Any(c => c.Id.Equals(id, StringComparison.Ordinal));

    /// <inheritdoc />
    public bool ContainsVersionString(string id, string version) => Connectors.Values.Any(
        c => c.VersionString().Equals($"{id} {version}", StringComparison.Ordinal)
    );

    /// <inheritdoc />
    public bool TryGetSettings(string name, out ConnectorSettings settings) =>
        Connectors.TryGetValue(name, out settings!);

    /// <inheritdoc />
    public bool TryGetSettingsById(string id, out ConnectorSettings[] settings)
    {
        settings = Connectors.Values.Where(c => c.Id.Equals(id, StringComparison.Ordinal))
            .ToArray();

        return settings.Length > 0;
    }

    /// <inheritdoc />
    public virtual ConnectorSettings this[string name]
    {
        get => Connectors[name];
        set => Connectors[name] = value;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, ConnectorSettings>> GetEnumerator() =>
        Connectors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
