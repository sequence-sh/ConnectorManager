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
