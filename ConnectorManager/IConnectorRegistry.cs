using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;
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
    /// <param name="prerelease"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IEnumerable<IPackageSearchMetadata>> Find(
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
    Task<bool> Exists(string id, NuGetVersion version, CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="prerelease"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IEnumerable<IPackageSearchMetadata>> GetMetadata(
        string id,
        bool prerelease = false,
        CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IEnumerable<NuGetVersion>> GetVersion(string id, CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="prerelease"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<NuGetVersion> GetLatestVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="force"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<PackageArchiveReader> GetConnectorPackage(
        string id,
        NuGetVersion version,
        bool force = false,
        CancellationToken ct = default);
}

}
