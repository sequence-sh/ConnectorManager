namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// 
/// </summary>
public interface IConnectorRegistrySettings
{
    /// <summary>
    /// URI of the connector registry.
    /// </summary>
    string Uri { get; set; }

    /// <summary>
    /// The username for private registries.
    /// </summary>
    string? RegistryUser { get; set; }

    /// <summary>
    /// The password / access token for private registries.
    /// </summary>
    string? RegistryToken { get; set; }
}

}
