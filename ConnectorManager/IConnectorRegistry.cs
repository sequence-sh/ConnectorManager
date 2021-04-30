using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

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
    /// <param name="ct"></param>
    /// <param name="prerelease"></param>
    /// <returns></returns>
    Task<IEnumerable<IPackageSearchMetadata>> Find(
        string search,
        CancellationToken ct,
        bool prerelease = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> Exists(string id, NuGetVersion version, CancellationToken ct);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <param name="prerelease"></param>
    /// <returns></returns>
    Task<IEnumerable<IPackageSearchMetadata>> GetMetadata(
        string id,
        CancellationToken ct,
        bool prerelease = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IEnumerable<NuGetVersion>> GetVersion(string id, CancellationToken ct);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <param name="prerelease"></param>
    /// <returns></returns>
    Task<NuGetVersion> GetLatestVersion(string id, CancellationToken ct, bool prerelease = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="path"></param>
    /// <param name="ct"></param>
    /// <param name="force"></param>
    /// <returns></returns>
    Task<PackageIdentity?> Install(
        string id,
        NuGetVersion version,
        string path,
        CancellationToken ct,
        bool force = false);
}

}
