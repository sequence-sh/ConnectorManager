using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Extension methods for dependency injection using IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Create all the required services to set up a ConnectorManager using
    /// a JSON configuration file.
    /// 
    ///   - ConnectorManagerSettings
    ///   - ConnectorRegistrySettings
    ///   - IConnectorRegistry
    ///   - IConnectorConfiguration (FileConnectorConfiguration)
    ///   - IConnectorManager
    /// 
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configuration">The application Configuration.</param>
    /// <param name="fileSystem">The file system.</param>
    /// <returns></returns>
    public static IServiceCollection AddConnectorManager(
        this IServiceCollection services,
        IConfiguration configuration,
        IFileSystem fileSystem)
    {
        var managerSettings = configuration.GetSection(ConnectorManagerSettings.Key)
            .Get<ConnectorManagerSettings>() ?? ConnectorManagerSettings.Default;

        services.AddSingleton(managerSettings);

        var registrySettings = configuration.GetSection(ConnectorRegistrySettings.Key)
            .Get<ConnectorRegistrySettings>() ?? ConnectorRegistrySettings.Reductech;

        services.AddSingleton(registrySettings);

        services.AddSingleton<IConnectorRegistry, ConnectorRegistry>();

        var connectorConfig =
            FileConnectorConfiguration.CreateFromJson(managerSettings, fileSystem);

        connectorConfig.Wait();

        services.AddSingleton<IConnectorConfiguration>(connectorConfig.Result);

        services.AddSingleton<IConnectorManager, ConnectorManager>();

        return services;
    }
}

}
