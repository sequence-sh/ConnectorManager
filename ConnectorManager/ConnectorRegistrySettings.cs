namespace Reductech.Sequence.ConnectorManagement;

/// <summary>
/// Defines uri and login details for a connector registry.
/// </summary>
public record ConnectorRegistryEndpoint
{
    /// <summary>
    /// The uri of the connector registry.
    /// </summary>
    public string Uri { get; init; } = null!;

    /// <summary>
    /// UserName for private registries.
    /// </summary>
    public string? User { get; init; }

    /// <summary>
    /// Token / password for private registries.
    /// </summary>
    public string? Token { get; init; }
}

/// <summary>
/// Settings for the default Reductech connector registry.
/// </summary>
public record ConnectorRegistrySettings
{
    /// <summary>
    /// appsettings.json section key that contains the settings
    /// </summary>
    public const string Key = "connectorRegistry";

    /// <summary>
    /// Default settings for the Reductech Connector Registry.
    /// </summary>
    public static ConnectorRegistrySettings Reductech = new()
    {
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
    /// Collection of connector registries.
    /// </summary>
    public ConnectorRegistryEndpoint[] Registries { get; init; } = null!;

    /// <summary>
    /// By default, nuget protocol logging is disabled. For debugging, set this to true.
    /// </summary>
    public bool EnableNuGetLog { get; init; } = false;
}
