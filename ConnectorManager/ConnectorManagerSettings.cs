using System.IO;

namespace Reductech.EDR.ConnectorManagement;

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
    /// Path to the installation directory for connectors.
    /// </summary>
    public string ConnectorPath { get; init; }

    /// <summary>
    /// Path to the connector configuration JSON.
    /// </summary>
    public string ConfigurationPath { get; init; }

    /// <summary>
    /// Automatically download missing connectors when using the Verify method.
    /// </summary>
    public bool AutoDownload { get; init; }
}
