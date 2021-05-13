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
    /// Additional services required for the connector manager are:
    ///
    ///    - System.IO.Abstractions.IFileSystem
    ///    - Microsoft.Extensions.Logging.ILogger
    /// 
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configuration">The application Configuration.</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddConnectorManager(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var managerSettings = configuration.GetSection(ConnectorManagerSettings.Key)
            .Get<ConnectorManagerSettings>() ?? ConnectorManagerSettings.Default;

        services.AddSingleton(managerSettings);

        var registrySettings = configuration.GetSection(ConnectorRegistrySettings.Key)
            .Get<ConnectorRegistrySettings>() ?? ConnectorRegistrySettings.Reductech;

        services.AddSingleton(registrySettings);

        services.AddSingleton<IConnectorRegistry, ConnectorRegistry>();

        services.AddSingleton<IConnectorConfiguration, FileConnectorConfiguration>();

        services.AddSingleton<IConnectorManager, ConnectorManager>();

        return services;
    }
}

}
