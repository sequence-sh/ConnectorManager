using System.Collections.Generic;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// A way of managing connector configurations.
/// </summary>
public interface IConnectorConfiguration : IEnumerable<KeyValuePair<string, ConnectorSettings>>
{
    /// <summary>
    /// Gets a collection containing the connector settings.
    /// </summary>
    ICollection<ConnectorSettings> Connectors { get; }

    /// <summary>
    /// Gets the number of configurations in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Add a connector configuration to the collection.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="settings"></param>
    void Add(string name, ConnectorSettings settings);

    /// <summary>
    /// Remove a connector configuration from the collection.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    bool Remove(string name);

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
    bool TryGetValue(string name, out ConnectorSettings settings);

    /// <summary>
    /// Gets or sets the configuration with the specified name.
    /// </summary>
    /// <param name="name"></param>
    ConnectorSettings this[string name] { get; set; }
}

}
