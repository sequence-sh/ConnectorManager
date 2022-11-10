using Sequence.ConnectorManagement.Base;

namespace Sequence.ConnectorManagement;

/// <summary>
/// Represents a way of interacting with a remote connector registry / nuget feed.
/// </summary>
public interface IConnectorRegistry
{
    /// <summary>
    /// Searches connector package IDs for the specified search string.
    /// Empty string will return all results.
    /// </summary>
    /// <param name="search">Search string</param>
    /// <param name="prerelease">Include prerelease packages when searching</param>
    /// <param name="ct">The cancellation token. Default is None.</param>
    /// <returns></returns>
    Task<ICollection<ConnectorMetadata>> Find(
        string search,
        bool prerelease = false,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a connector package with the specified id or id-version exists
    /// in the connector registry.
    /// </summary>
    /// <param name="id">Id (unique name) of the connector package</param>
    /// <param name="version">Connector version</param>
    /// <param name="ct">The cancellation token. Default is None.</param>
    /// <returns></returns>
    Task<bool> Exists(string id, string? version = null, CancellationToken ct = default);

    /// <summary>
    /// Returns a list of all versions for the specified connector id.
    /// The collection will be empty if the id does not exist in the registry.
    /// </summary>
    /// <param name="id">Id (unique name) of the connector package</param>
    /// <param name="prerelease">Include prerelease versions in results</param>
    /// <param name="ct">The cancellation token. Default is None.</param>
    /// <returns>A sorted list of versions, with the latest version last.</returns>
    Task<ICollection<string>> GetVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves metadata and connector package with the specified id and version.
    /// </summary>
    /// <param name="id">Id (unique name) of the connector package</param>
    /// <param name="version">Connector version</param>
    /// <param name="ct">The cancellation token. Default is None.</param>
    /// <returns></returns>
    Task<ConnectorPackage> GetConnectorPackage(
        string id,
        string version,
        CancellationToken ct = default);
}
