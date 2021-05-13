using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Contains extension methods for working with connector managers.
/// </summary>
public static class ConnectorManagerExtensions
{
    /// <summary>
    /// Gets a StepFactory store from a connector manager
    /// </summary>
    /// <returns></returns>
    public static async Task<StepFactoryStore> GetStepFactoryStoreAsync(
        this IConnectorManager connectorManager,
        CancellationToken ct = default,
        params ConnectorData[] additionalConnectors)
    {
        if (!await connectorManager.Verify(ct))
            throw new ConnectorConfigurationException("Could not validate installed connectors.");

        var connectors = connectorManager.List()
            .Select(c => c.data)
            .Where(c => c.ConnectorSettings.Enable)
            .ToArray();

        if (connectors.GroupBy(c => c.ConnectorSettings.Id).Any(g => g.Count() > 1))
            throw new ConnectorConfigurationException(
                "When using multiple configurations with the same connector id, at most one can be enabled."
            );

        var stepFactoryStore =
            StepFactoryStore.Create(connectors.Concat(additionalConnectors).ToArray());

        return stepFactoryStore;
    }

    /// <summary>
    /// Represents errors that occur when configuring or validating connectors.
    /// </summary>
    public class ConnectorConfigurationException : Exception
    {
        /// <inheritdoc />
        public ConnectorConfigurationException(string message) : base(message) { }
    }
}

}
