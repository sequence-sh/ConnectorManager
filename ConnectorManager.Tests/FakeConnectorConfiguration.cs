using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public class FakeConnectorConfiguration : ConnectorConfigurationBase
{
    protected override Dictionary<string, ConnectorSettings> Connectors { get; } = new()
    {
        {
            "Reductech.EDR.Connectors.Nuix",
            new ConnectorSettings { Id = "Reductech.EDR.Connectors.Nuix", Version = "0.9.0" }
        },
        {
            "Reductech.EDR.Connectors.StructuredData",
            new ConnectorSettings
            {
                Id = "Reductech.EDR.Connectors.StructuredData", Version = "0.9.0"
            }
        }
    };

    public override Task AddAsync(
        string name,
        ConnectorSettings settings,
        CancellationToken ct = default)
    {
        Connectors.Add(name, settings);
        return Task.CompletedTask;
    }

    public override Task<bool> RemoveAsync(string name, CancellationToken ct = default) =>
        Task.FromResult(Connectors.Remove(name));
}

}
