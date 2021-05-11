namespace Reductech.EDR.ConnectorManagement
{

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
        Uri   = "https://gitlab.com/api/v4/projects/26337972/packages/nuget/index.json",
        User  = "connectormanager",
        Token = "DdGjUmoo-oqbcBoCd9pE"
    };

    /// <summary>
    /// 
    /// </summary>
    public string Uri { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string? User { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string? Token { get; init; }
}

}
