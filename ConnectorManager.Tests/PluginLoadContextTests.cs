﻿using System.IO;
using MELT;

namespace Sequence.ConnectorManagement.Tests;

public class PluginLoadContextTests
{
    private static readonly string RelativePath = Path.Combine(
        "ConnectorManager.Tests",
        "ExampleConnector",
        "ExampleConnector.dll"
    );

    private readonly ILoggerFactory _loggerFactory;

    public PluginLoadContextTests()
    {
        _loggerFactory = TestLoggerFactory.Create();
    }

    [Fact]
    public void LoadPlugin_WhenAssemblyExists_LoadsAssembly()
    {
        var loggerFactory = TestLoggerFactory.Create();

        var absolutePath = PluginLoadContext.GetAbsolutePath(RelativePath);

        var assembly = PluginLoadContext.LoadPlugin(
            absolutePath,
            _loggerFactory.CreateLogger("test")
        );

        Assert.NotNull(assembly);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.Message!.Equals($"Successfully loaded assembly: {assembly.FullName}")
        );
    }
}
