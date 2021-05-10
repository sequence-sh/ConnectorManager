using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reductech.EDR.Core.Internal;

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
    /// <param name="fileSystem">The file system.</param>
    /// <param name="defaultConnectorSettings">Default connector configuration to create if a configuration file is not found.</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddConnectorManager(
        this IServiceCollection services,
        IConfiguration configuration,
        IFileSystem fileSystem,
        Dictionary<string, ConnectorSettings>? defaultConnectorSettings = null)
    {
        var managerSettings = configuration.GetSection(ConnectorManagerSettings.Key)
            .Get<ConnectorManagerSettings>() ?? ConnectorManagerSettings.Default;

        services.AddSingleton(managerSettings);

        var registrySettings = configuration.GetSection(ConnectorRegistrySettings.Key)
            .Get<ConnectorRegistrySettings>() ?? ConnectorRegistrySettings.Reductech;

        services.AddSingleton(registrySettings);

        services.AddSingleton<IConnectorRegistry, ConnectorRegistry>();

        var connectorConfig = defaultConnectorSettings == null
            ? FileConnectorConfiguration.CreateFromJson(managerSettings, fileSystem)
            : FileConnectorConfiguration.Create(
                managerSettings,
                fileSystem,
                defaultConnectorSettings
            );

        connectorConfig.Wait();

        services.AddSingleton<IConnectorConfiguration>(connectorConfig.Result);

        services.AddSingleton<IConnectorManager, ConnectorManager>();

        return services;
    }
}

}
