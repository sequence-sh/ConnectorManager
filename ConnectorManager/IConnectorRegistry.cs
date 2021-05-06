using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Represents a way of interacting with a remote connector registry / nuget feed.
/// </summary>
public interface IConnectorRegistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="search"></param>
    /// <param name="prerelease"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<ICollection<ConnectorMetadata>> Find(
        string search,
        bool prerelease = false,
        CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> Exists(string id, string? version = null, CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="prerelease"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<ICollection<string>> GetVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<ConnectorPackage> GetConnectorPackage(
        string id,
        string version,
        CancellationToken ct = default);
}

}
