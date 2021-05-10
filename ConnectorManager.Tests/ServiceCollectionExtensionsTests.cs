using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reductech.EDR.Core.Internal;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public class ServiceCollectionExtensions
{
    private static (ServiceProvider, MockFileSystem) CreateServiceCollection(
        Dictionary<string, ConnectorSettings>? connectorSettings = null)
    {
        var services = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();

        var fs = new MockFileSystem();

        fs.AddDirectory(
            fs.Path.GetDirectoryName(ConnectorManagerSettings.Default.ConfigurationPath)
        );

        fs.AddDirectory(ConnectorManagerSettings.Default.ConnectorPath);

        services.AddSingleton<IFileSystem>(fs);

        services.AddLogging();

        services.AddConnectorManager(config, fs, connectorSettings);

        var provider = services.BuildServiceProvider();

        return (provider, fs);
    }

    [Fact]
    public void AddConnectorManager_ByDefault_CreateConnectorManagerWithEmptyConfiguration()
    {
        var (provider, fs) = CreateServiceCollection();

        Assert.NotNull(provider.GetService(typeof(ConnectorManagerSettings)));
        Assert.NotNull(provider.GetService(typeof(ConnectorRegistrySettings)));
        Assert.NotNull(provider.GetService(typeof(IConnectorRegistry)));
        Assert.NotNull(provider.GetService(typeof(IConnectorConfiguration)));
        Assert.NotNull(provider.GetService(typeof(IConnectorManager)));

        var configFile = fs.GetFile(ConnectorManagerSettings.Default.ConfigurationPath);

        Assert.NotNull(configFile);
        Assert.Equal("{}", configFile.TextContents);
    }

    [Fact]
    public void AddConnectorManager_WithDefaultSettings_CreatesConfiguration()
    {
        const string connectorName = "EDR.Connector";

        var connectorSettings = new Dictionary<string, ConnectorSettings>
        {
            { connectorName, new ConnectorSettings { Id = connectorName, Version = "0.1.0" } }
        };

        var (provider, fs) = CreateServiceCollection(connectorSettings);

        var configFile = fs.GetFile(ConnectorManagerSettings.Default.ConfigurationPath);

        Assert.NotNull(configFile);
        Assert.Matches("Connector", configFile.TextContents);

        var config = (IConnectorConfiguration)provider.GetService(typeof(IConnectorConfiguration));

        Assert.Contains(connectorName, config.Keys);
    }
}

}
