using System.IO;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// 
/// </summary>
public record ConnectorPackage(string Id, string Version, MemoryStream? Stream = null) { }

}
