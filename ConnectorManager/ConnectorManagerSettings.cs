using System;
using System.IO;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Settings for the connector manager.
/// </summary>
public record ConnectorManagerSettings
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
    public static ConnectorManagerSettings Default = new()
    {
        ConnectorPath = Path.Combine(
            AppContext.BaseDirectory,
            "connectors"
        ),
        ConfigurationPath = Path.Combine(
            AppContext.BaseDirectory,
            "connectors.json"
        ),
        AutoDownload = true
    };

    /// <summary>
    /// 
    /// </summary>
    public string ConnectorPath { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string ConfigurationPath { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public bool AutoDownload { get; init; }
}

}
