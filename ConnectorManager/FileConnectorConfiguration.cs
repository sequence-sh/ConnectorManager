using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
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
public class FileConnectorConfiguration : ConnectorConfigurationBase
{
    private readonly string _path;
    private readonly IFileSystem _fileSystem;

    private FileConnectorConfiguration(
        string path,
        Dictionary<string, ConnectorSettings> connectors,
        IFileSystem fileSystem) : base(connectors)
    {
        _path       = path;
        _fileSystem = fileSystem;
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
        var jsonSettings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters        = new List<JsonConverter> { EntityJsonConverter.Instance }
            };

        var output = JsonConvert.SerializeObject(
            Connectors,
            Formatting.Indented,
            jsonSettings
        );

        await _fileSystem.File.WriteAllTextAsync(_path, output, ct);
    }

    /// <summary>
    /// Create a new ConnectorConfiguration using a JSON file.
    /// </summary>
    public static async Task<IConnectorConfiguration> FromJson(
        ConnectorManagerSettings settings,
        IFileSystem fileSystem)
    {
        if (!fileSystem.File.Exists(settings.ConfigurationPath))
            throw new FileNotFoundException(
                "Connector configuration file not found.",
                settings.ConfigurationPath
            );

        var text = await fileSystem.File.ReadAllTextAsync(settings.ConfigurationPath);

        var connectors = JsonConvert.DeserializeObject<Dictionary<string, ConnectorSettings>>(
            text,
            EntityJsonConverter.Instance
        ) ?? new Dictionary<string, ConnectorSettings>();

        return new FileConnectorConfiguration(settings.ConfigurationPath, connectors, fileSystem);
    }
}

}
