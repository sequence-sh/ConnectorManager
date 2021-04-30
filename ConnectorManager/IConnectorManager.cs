using System.Threading;
using System.Threading.Tasks;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// 
/// </summary>
public interface IConnectorManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="ct"></param>
    /// <param name="prerelease"></param>
    /// <param name="force"></param>
    /// <returns></returns>
    Task Add(
        string id,
        string? version,
        CancellationToken ct,
        bool prerelease = false,
        bool force = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="ct"></param>
    /// <param name="prerelease"></param>
    /// <returns></returns>
    Task Update(string id, string? version, CancellationToken ct, bool prerelease = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    void Remove(string id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    void List(string? filter);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="search"></param>
    /// <param name="ct"></param>
    /// <param name="prerelease"></param>
    /// <returns></returns>
    Task Find(string? search, CancellationToken ct, bool prerelease = false);
}

}
