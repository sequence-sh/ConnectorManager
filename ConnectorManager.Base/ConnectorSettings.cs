using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Reductech.EDR.ConnectorManagement.Base
{

/// <summary>
/// Settings for a connector
/// </summary>
[DataContract]
public class ConnectorSettings
{
    /// <summary>
    /// The id of the connector in the registry
    /// </summary>
    [DataMember(Name = "id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// The version of the connector
    /// </summary>
    [DataMember(Name = "version")]
    public string Version { get; set; } = null!;

    /// <summary>
    /// Whether to enable this connector
    /// </summary>
    [DataMember(Name = "enable")]
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Individual settings for the connector
    /// </summary>
    [DataMember(Name = "settings")]
    public Dictionary<string, object>? Settings { get; set; }

    /// <summary>
    /// String representation
    /// </summary>
    /// <returns></returns>
    public string VersionString() => $"{Id} {Version}";

    /// <inheritdoc />
    public override string ToString() => VersionString();

    /// <summary>
    /// Create default settings for an assembly.
    /// </summary>
    public static ConnectorSettings DefaultForAssembly(Assembly assembly)
    {
        var versionAttribute =
            assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as
                AssemblyInformationalVersionAttribute;

        var version = versionAttribute?.InformationalVersion;

        return new ConnectorSettings
        {
            Id      = assembly.GetName().Name ?? "Unknown",
            Version = version ?? "Unknown",
            Enable  = true
        };
    }
}

}
