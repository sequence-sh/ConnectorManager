using Newtonsoft.Json;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Settings for the default Reductech connector registry.
/// </summary>
public record ConnectorRegistrySettings(
    [JsonProperty("registry")] string Uri,
    [JsonProperty("registryUser")] string? RegistryUser = null,
    [JsonProperty("registryToken")] string? RegistryToken = null)
{
    /// <summary>
    /// appsettings.json section key that contains the settings
    /// </summary>
    public const string Key = "connectorRegistry";

    /// <summary>
    /// Default settings for the Reductech Connector Registry.
    /// </summary>
    public static ConnectorRegistrySettings Reductech = new(
        "https://gitlab.com/api/v4/projects/26337972/packages/nuget/index.json",
        "connectormanager",
        "DdGjUmoo-oqbcBoCd9pE"
    );
}

}
