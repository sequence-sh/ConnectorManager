using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.ConnectorManagement.Base;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// A ConnectorConfiguration that uses an in-memory dictionary as the backing store.
/// </summary>
public class ConnectorConfiguration : ConnectorConfigurationBase
{
    /// <inheritdoc />
    protected override Dictionary<string, ConnectorSettings> Connectors { get; }

    /// <summary>
    /// Create a new empty configuration
    /// </summary>
    public ConnectorConfiguration() => Connectors = new Dictionary<string, ConnectorSettings>();

    /// <summary>
    /// Create a new configuration with the specified connector settings.
    /// </summary>
    /// <param name="connectors"></param>
    public ConnectorConfiguration(Dictionary<string, ConnectorSettings> connectors) =>
        Connectors = connectors;

    /// <inheritdoc />
    public override Task AddAsync(
        string name,
        ConnectorSettings settings,
        CancellationToken ct = default)
    {
        Connectors.Add(name, settings);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task<bool> RemoveAsync(string name, CancellationToken ct = default) =>
        Task.FromResult(Connectors.Remove(name));
}

}
