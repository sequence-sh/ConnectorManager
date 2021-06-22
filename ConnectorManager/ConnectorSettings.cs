using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Settings for a connector
/// </summary>
[Serializable]
public class ConnectorSettings
{
    /// <summary>
    /// The id of the connector in the registry
    /// </summary>
    [JsonProperty(propertyName: "id")]
    public string Id { get; set; }

    /// <summary>
    /// The version of the connector
    /// </summary>
    [JsonProperty(propertyName: "version")]
    public string Version { get; set; }

    /// <summary>
    /// Whether to enable this connector
    /// </summary>
    [JsonProperty(propertyName: "enable")]
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Individual settings for the connector
    /// </summary>
    [JsonProperty(propertyName: "settings")]
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
