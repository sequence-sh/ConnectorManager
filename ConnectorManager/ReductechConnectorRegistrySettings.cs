using Newtonsoft.Json;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Configuration for the default Reductech connector registry.
/// </summary>
public class ReductechConnectorRegistrySettings : IConnectorRegistrySettings
{
    /// <summary>
    /// appsettings.json key that contains ReductechConnectorRegistrySettings
    /// </summary>
    public const string Key = "connectorRegistry";

    /// <inheritdoc />
    [JsonProperty(PropertyName = "registry")]
    public string Uri { get; set; } =
        "https://gitlab.com/api/v4/projects/18697166/packages/nuget/index.json";

    /// <inheritdoc />
    [JsonProperty(PropertyName = "registryUser")]
    public string? RegistryUser { get; set; } = "project_18697166_bot";

    /// <inheritdoc />
    [JsonProperty(PropertyName = "registryToken")]
    public string? RegistryToken { get; set; } = "drw__6yo8Vym_EqnoGwb";
}

}
