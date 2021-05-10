using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// 
/// </summary>
public interface IConnectorManager
{
    /// <summary>
    /// Add a connector with the specified id to the configuration.
    /// </summary>
    /// <param name="id">Connector registry Id</param>
    /// <param name="name">Name of the configuration to add</param>
    /// <param name="version">Connector version</param>
    /// <param name="prerelease">Allow prerelease versions to be used</param>
    /// <param name="force">Re-add if connector configuration already exists</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    Task Add(
        string id,
        string? name = null,
        string? version = null,
        bool prerelease = false,
        bool force = false,
        CancellationToken ct = default);

    /// <summary>
    /// Update the connector configuration with the specified name.
    /// </summary>
    /// <param name="name">Name of the configuration to update</param>
    /// <param name="version">Connector version</param>
    /// <param name="prerelease">Allow prerelease versions to be used</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    Task Update(
        string name,
        string? version = null,
        bool prerelease = false,
        CancellationToken ct = default);

    /// <summary>
    /// Remove the connector configuration with the specified name.
    /// </summary>
    /// <param name="name">Name of the configuration to remove</param>
    /// <param name="configurationOnly">Do not remove the connector directory</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    Task Remove(string name, bool configurationOnly = false, CancellationToken ct = default);

    /// <summary>
    /// List all available connector configurations.
    /// </summary>
    /// <param name="nameFilter">If specified, only configuration names matching this regular expression will be shown</param>
    IEnumerable<(string name, ConnectorData data)> List(string? nameFilter = null);

    /// <summary>
    /// Find or list connectors available in the connector registry.
    /// </summary>
    /// <param name="search">If specified, only connectors mathing this search string will be shown</param>
    /// <param name="prerelease">Allow prerelease versions to be used</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    Task<ICollection<ConnectorMetadata>> Find(
        string? search = null,
        bool prerelease = false,
        CancellationToken ct = default);

    /// <summary>
    /// Verifies that connectors defined in the configuration have been installed.
    /// Does a basic check for the directory and connector dll only, does not verify
    /// if the connector package has all the correct dependencies.
    /// If ConnectorManagerSettings.AutoDownload is true, installs missing connectors.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    Task<bool> Verify(CancellationToken ct = default);
}

}
