using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// A way of managing connector configurations.
/// </summary>
public interface IConnectorConfiguration : IEnumerable<KeyValuePair<string, ConnectorSettings>>
{
    /// <summary>
    /// Gets a collection containing the names of the configurations.
    /// </summary>
    ICollection<string> Keys { get; }

    /// <summary>
    /// Gets a collection containing the connector settings.
    /// </summary>
    ICollection<ConnectorSettings> Settings { get; }

    /// <summary>
    /// Gets the number of configurations in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Add a connector configuration to the collection.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="settings"></param>
    /// <param name="ct"></param>
    Task AddAsync(string name, ConnectorSettings settings, CancellationToken ct);

    /// <summary>
    /// Remove a connector configuration from the collection.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> RemoveAsync(string name, CancellationToken ct);

    /// <summary>
    /// Determines if a configuration with the specified name exists.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    bool Contains(string name);

    /// <summary>
    /// Determines if a configuration with the specified id exists.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool ContainsId(string id);

    /// <summary>
    /// Determines if a configuration with the specified id and version exists.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    bool ContainsVersionString(string id, string version);

    /// <summary>
    /// Gets the configuration associated with the specified name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    bool TryGetSettings(string name, out ConnectorSettings settings);

    /// <summary>
    /// Gets the configuration(s) associated with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    bool TryGetSettingsById(string id, out ConnectorSettings[] settings);

    /// <summary>
    /// Gets or sets the configuration with the specified name.
    /// </summary>
    /// <param name="name"></param>
    ConnectorSettings this[string name] { get; set; }
}

}
