using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.ConnectorManagement.Tests;

public class ServiceCollectionExtensions
{
    private static (ServiceProvider, MockFileSystem) CreateServiceCollection()
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

        services.AddConnectorManager(config);

        var provider = services.BuildServiceProvider();

        return (provider, fs);
    }

    [Fact]
    public void AddConnectorManager_ByDefault_CreatesTheRequiredServices()
    {
        var (provider, _) = CreateServiceCollection();

        Assert.NotNull(provider.GetService(typeof(ConnectorManagerSettings)));
        Assert.NotNull(provider.GetService(typeof(ConnectorRegistrySettings)));
        Assert.NotNull(provider.GetService(typeof(IConnectorRegistry)));
        Assert.NotNull(provider.GetService(typeof(IConnectorConfiguration)));
        Assert.NotNull(provider.GetService(typeof(IConnectorManager)));
    }

    [Fact]
    public void AddConnectorManager_CorrectlySerializesCustomAppSettingsJson()
    {
        const string settingsJson = @"{
  ""connectorRegistry"": {
    ""registries"": [
        {
            ""uri"": ""https://registry/packages/index.json"",
            ""user"": ""connectoruser""
        }
    ]
  },
  ""connectorManager"": {
    ""connectorPath"": ""c:\\connectors"",
    ""configurationPath"": ""c:\\connectors\\connectors.json"",
    ""autoDownload"": false
  }
}";

        var fs         = new MockFileSystem();
        var configPath = fs.Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        fs.AddFile(configPath, settingsJson);

        fs.AddDirectory("c:\\connectors");

        var hb = new HostBuilder().ConfigureAppConfiguration(
                (_, config) => config.AddJsonStream(fs.File.OpenRead(configPath))
            )
            .ConfigureServices(
                (context, services) =>
                {
                    services.AddSingleton<IFileSystem>(fs);
                    services.AddLogging();
                    services.AddConnectorManager(context.Configuration);
                }
            )
            .Build();

        var registryConfig =
            (ConnectorRegistrySettings)hb.Services.GetService(typeof(ConnectorRegistrySettings))!;

        Assert.Equal("https://registry/packages/index.json", registryConfig.Registries[0].Uri);
        Assert.Equal("connectoruser",                        registryConfig.Registries[0].User);
        Assert.Null(registryConfig.Registries[0].Token);

        var managerConfig =
            (ConnectorManagerSettings)hb.Services.GetService(typeof(ConnectorManagerSettings))!;

        Assert.Equal("c:\\connectors",                  managerConfig.ConnectorPath);
        Assert.Equal("c:\\connectors\\connectors.json", managerConfig.ConfigurationPath);
        Assert.False(managerConfig.AutoDownload);
    }
}
