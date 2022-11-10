using System.IO;

namespace Sequence.ConnectorManagement;

/// <summary>
/// Settings for the connector manager.
/// </summary>
public record ConnectorManagerSettings
{
    /// <summary>
    /// appsettings.json section key that contains the settings
    /// </summary>
    public const string Key = "connectorManager";

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
        Registries = new ConnectorRegistryEndpoint[]
        {
            new()
            {
                Uri =
                    "https://gitlab.com/api/v4/projects/26337972/packages/nuget/index.json"
            },
            new()
            {
                Uri =
                    "https://gitlab.com/api/v4/projects/35096937/packages/nuget/index.json"
            }
        }
    };

    /// <summary>
    /// Path to the installation directory for connectors.
    /// </summary>
    public string ConnectorPath { get; init; } = null!;

    /// <summary>
    /// Path to the connector configuration JSON.
    /// </summary>
    public string ConfigurationPath { get; init; } = null!;

    /// <summary>
    /// Automatically download missing connectors when using the Verify method.
    /// </summary>
    public bool AutoDownload { get; init; } = true;

    /// <summary>
    /// Collection of connector registries.
    /// </summary>
    public ConnectorRegistryEndpoint[] Registries { get; init; } = null!;

    /// <summary>
    /// By default, nuget protocol logging is disabled. For debugging, set this to true.
    /// </summary>
    public bool EnableNuGetLog { get; init; } = false;
}
