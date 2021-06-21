using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.ConnectorManagement
{

/// <summary>
/// Loads assemblies for connector plugins.
/// </summary>
public class PluginLoadContext : AssemblyLoadContext
{
    private PluginLoadContext(string pluginPath)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    private readonly AssemblyDependencyResolver _resolver;

    /// <summary>
    /// Gets an absolute path from a path relative to this assembly
    /// </summary>
    public static string GetAbsolutePath(string relativePath)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly()!.Location;
        var directory        = GoUp(assemblyLocation, 5)!;

        var result = Path.Combine(directory, relativePath);

        return result;

        static string GoUp(string path, int levels)
        {
            var current = path;

            for (var i = 0; i < levels; i++)
                current = Path.GetDirectoryName(current);

            return current!;
        }
    }

    /// <summary>
    /// Try to load a plugin from a path
    /// </summary>
    public static Assembly LoadPlugin(string absolutePath, ILogger logger)
    {
        logger.LogDebug($"Loading assembly from path: {absolutePath}");

        PluginLoadContext loadContext = new(absolutePath);

        var assembly = loadContext.LoadFromAssemblyName(
            new AssemblyName(Path.GetFileNameWithoutExtension(absolutePath))
        );

        logger.LogDebug($"Successfully loaded assembly: {assembly.FullName}");

        return assembly;
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

        return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
    }

    /// <inheritdoc/>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        return libraryPath is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(libraryPath);
    }
}

}
