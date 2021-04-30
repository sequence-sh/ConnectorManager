using System;
using System.IO;

namespace Reductech.EDR.ConnectorManagement
{

/// <inheritdoc />
public class ConnectorManagerSettings : IConnectorManagerSettings
{
    /// <summary>
    /// 
    /// </summary>
    public const string Key = "edr";

    /// <inheritdoc />
    public string ConnectorPath { get; set; } = Path.Combine(
        AppContext.BaseDirectory,
        "connectors"
    );

    /// <inheritdoc />
    public string ConfigurationPath { get; set; } = Path.Combine(
        AppContext.BaseDirectory,
        //"connectors",
        "connectors.json"
    );

    /// <inheritdoc />
    public bool AutoDownload { get; set; } = true;
}

}
