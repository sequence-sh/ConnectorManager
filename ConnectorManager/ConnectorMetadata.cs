using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// 
/// </summary>
public record ConnectorMetadata(string Id, string Version)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    public string GetInstallPath(string? root = null) =>
        root == null ? Path.Combine(Id, Version) : Path.Combine(root, Id, Version);

    /// <summary>
    /// 
    /// </summary>
    public List<string>? Files { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public List<ZipArchiveEntry>? Entries { get; set; } = null;
}

}
