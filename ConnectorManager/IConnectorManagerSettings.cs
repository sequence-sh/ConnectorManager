namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// 
/// </summary>
public interface IConnectorManagerSettings
{
    /// <summary>
    /// 
    /// </summary>
    string ConnectorPath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    string ConfigurationPath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    bool AutoDownload { get; set; }
}

}
