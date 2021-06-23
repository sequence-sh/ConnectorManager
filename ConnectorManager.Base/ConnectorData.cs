using System.Reflection;

namespace Reductech.EDR.ConnectorManagement.Base
{

/// <summary>
/// Connector assembly and associated settings.
/// </summary>
public record ConnectorData(ConnectorSettings ConnectorSettings, Assembly? Assembly)
{
    /// <inheritdoc />
    public override string ToString() => ConnectorSettings.ToString()!;
}

}
