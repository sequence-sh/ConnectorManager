using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public class FakeConnectorConfiguration : ConnectorConfigurationBase
{
    private const string ConfigJson = @"
{
  ""Reductech.EDR.Connectors.Nuix"": {
    ""id"": ""Reductech.EDR.Connectors.Nuix"",
    ""version"": ""0.9.0""
  },
  ""Reductech.EDR.Connectors.StructuredData"": {
    ""id"": ""Reductech.EDR.Connectors.StructuredData"",
    ""version"": ""0.9.0""
  }
}";

    public static IConnectorConfiguration GetDefaultConfiguration() =>
        new FakeConnectorConfiguration(
            JsonConvert.DeserializeObject<Dictionary<string, ConnectorSettings>>(
                ConfigJson,
                EntityJsonConverter.Instance
            )!
        );

    private FakeConnectorConfiguration(Dictionary<string, ConnectorSettings> connectors) : base(
        connectors
    ) { }

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
