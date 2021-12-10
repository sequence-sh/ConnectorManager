using System.IO.Abstractions;
using System.Text.Json;

namespace Reductech.EDR.ConnectorManagement;

/// <summary>
/// A ConnectorConfiguration that uses a JSON file as the backing store.
/// </summary>
public class FileConnectorConfiguration : ConnectorConfigurationBase
{
    private readonly string _path;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="fileSystem"></param>
    public FileConnectorConfiguration(ConnectorManagerSettings settings, IFileSystem fileSystem)
    {
        _path       = settings.ConfigurationPath;
        _fileSystem = fileSystem;
    }

    private Dictionary<string, ConnectorSettings> _connectors = null!;

    /// <inheritdoc />
    protected override Dictionary<string, ConnectorSettings> Connectors
    {
        get
        {
            Initialize();
            return _connectors;
        }
    }

    /// <inheritdoc />
    public override async Task AddAsync(
        string name,
        ConnectorSettings settings,
        CancellationToken ct = default)
    {
        Connectors.Add(name, settings);
        await SaveSettings(ct);
    }

    /// <inheritdoc />
    public override async Task<bool> RemoveAsync(string name, CancellationToken ct = default)
    {
        var success = Connectors.Remove(name);

        if (success)
            await SaveSettings(ct);

        return success;
    }

    /// <inheritdoc />
    public override ConnectorSettings this[string name]
    {
        get => Connectors[name];
        set
        {
            Connectors[name] = value;
            SaveSettings(CancellationToken.None).Wait();
        }
    }

    private async Task SaveSettings(CancellationToken ct)
    {
        var options = new JsonSerializerOptions() { WriteIndented = true, IgnoreNullValues = true };

        var output =
            JsonSerializer.Serialize(
                Connectors,
                options
            );

        await _fileSystem.File.WriteAllTextAsync(_path, output, ct).ConfigureAwait(false);
    }

    private bool _init;

    private void Initialize()
    {
        if (_init)
            return;

        string text;

        if (_fileSystem.File.Exists(_path))
        {
            text = _fileSystem.File.ReadAllText(_path);
        }
        else
        {
            text = "{}";
            _fileSystem.File.WriteAllText(_path, text);
        }

        _connectors = JsonSerializer.Deserialize<Dictionary<string, ConnectorSettings>>(
            text,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }
        )!;

        _init = true;
    }
}
