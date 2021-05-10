using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="fileSystem"></param>
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

        // TODO: include this in the FromJson method. Rename to CreateOrLoadFromJson
        if (!fileSystem.File.Exists(managerSettings.ConfigurationPath))
            fileSystem.File.WriteAllText(managerSettings.ConfigurationPath, "{}");

        // TODO: Add a FromObject()
        var connectorConfig = FileConnectorConfiguration.FromJson(
            managerSettings.ConfigurationPath,
            fileSystem
        );

        connectorConfig.Wait();

        services.AddSingleton<IConnectorConfiguration>(connectorConfig.Result);

        services.AddSingleton<IConnectorManager, ConnectorManager>();

        return services;
    }
}

}
