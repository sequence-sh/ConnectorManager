using System.Threading.Tasks;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public class ConnectorConfigurationTests
{
    [Fact]
    public async Task AddAsync_AddsConfiguration()
    {
        const string name = "config";

        var config = new ConnectorConfiguration();

        await config.AddAsync(name, new ConnectorSettings { Id = "Connector", Version = "1.0.0" });

        Assert.Contains(name, config.Keys);
    }

    [Fact]
    public async Task RemoveAsync_RemovesConfiguration()
    {
        const string name = "Reductech.EDR.Connectors.Nuix";

        var config = new ConnectorConfiguration(Helpers.GetDefaultConnectors());

        Assert.Contains(name, config.Keys);

        await config.RemoveAsync(name);

        Assert.DoesNotContain(name, config.Keys);
    }
}

}
