using System;
using System.IO;
using Newtonsoft.Json;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Settings for the connector manager.
/// </summary>
public record ConnectorManagerSettings(
    [JsonProperty("connectorPath")] string ConnectorPath,
    [JsonProperty("configurationPath")] string ConfigurationPath,
    [JsonProperty("autoDownload")] bool AutoDownload)
{
    /// <summary>
    /// appsettings.json section key that contains the settings
    /// </summary>
    public const string Key = "edr";

    /// <summary>
    /// Default settings for the connector manager.
    /// Connectors are stored in .\connectors and the settings
    /// are in .\connectors.json
    /// </summary>
    public static ConnectorManagerSettings Default = new(
        Path.Combine(
            AppContext.BaseDirectory,
            "connectors"
        ),
        Path.Combine(
            AppContext.BaseDirectory,
            //"connectors",
            "connectors.json"
        ),
        true
    );
}

}
