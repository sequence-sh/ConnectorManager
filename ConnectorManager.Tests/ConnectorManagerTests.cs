using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using MELT;
using Moq;
using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.ConnectorManagement.Tests;

public partial class ConnectorManagerTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConnectorConfiguration _config;
    private readonly ConnectorManagerSettings _settings;
    private readonly MockFileSystem _fileSystem;
    private readonly IConnectorRegistry _registry;
    private readonly ConnectorManager _manager;

    public ConnectorManagerTests()
    {
        _loggerFactory = TestLoggerFactory.Create();
        _config        = new ConnectorConfiguration(Helpers.GetDefaultConnectors());
        _settings      = ConnectorManagerSettings.Default;
        _fileSystem    = new MockFileSystem();
        _registry      = new FakeConnectorRegistry();

        _manager = new ConnectorManager(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            _registry,
            _config,
            _fileSystem
        );
    }

    [Fact]
    public async Task List_WhenConfigurationIsEmpty_ReturnsEmpty()
    {
        await _config.RemoveAsync("Reductech.Sequence.Connectors.Nuix");
        await _config.RemoveAsync("Reductech.Sequence.Connectors.StructuredData");

        Assert.Empty(_config);

        var configs = _manager.List();

        Assert.Empty(configs);
    }

    [Fact]
    public void List_WhenDllDoesNotExist_WritesErrorAndContinues()
    {
        const string name    = "Reductech.Sequence.Connectors.Nuix";
        const string version = "0.13.0";

        var path = _fileSystem.Path.Combine(_settings.ConnectorPath, name, version);

        var mock = new Mock<ConnectorManager>(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            _registry,
            _config,
            _fileSystem
        );

        mock.SetupSequence(m => m.LoadPlugin(It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(() => throw new Exception())
            .Returns(Mock.Of<Assembly>);

        mock.CallBase = true;

        var configs = mock.Object.List().ToArray();

        Assert.Single(configs);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Contains(
                     $"Failed to load connector configuration '{name}' from '{path}'."
                 )
        );
    }

    [Fact]
    public void List_ByDefault_ReturnsConnectorAssemblies()
    {
        const string name    = "Reductech.Sequence.Connectors.Nuix";
        const string version = "0.13.0";
        const string filter  = "(?i)nuix";

        var mock = new Mock<ConnectorManager>(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            _registry,
            _config,
            _fileSystem
        );

        mock.Setup(m => m.LoadPlugin(It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(Mock.Of<Assembly>);

        var configs = mock.Object.List(filter).ToArray();

        Assert.Single(configs);

        Assert.Equal(name,    configs[0].name);
        Assert.Equal(name,    configs[0].data.ConnectorSettings.Id);
        Assert.Equal(version, configs[0].data.ConnectorSettings.Version);
    }

    [Fact]
    public async Task Find_ByDefault_ReturnsLatestMajorVersionOnly()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.Sequence.Connectors.FileSystem", "0.13.0"),
            new("Reductech.Sequence.Connectors.StructuredData", "0.13.0")
        };

        var connectors = await _manager.Find();

        Assert.Equal(expected, connectors);
    }

    [Fact]
    public async Task Find_WhenPrereleaseIsTrue_ReturnsLatestVersions()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.Sequence.Connectors.FileSystem", "0.13.0"),
            new("Reductech.Sequence.Connectors.StructuredData", "0.13.0"),
            new("Reductech.Sequence.Connectors.Nuix", "0.13.0-beta.2")
        };

        var connectors = await _manager.Find(prerelease: true);

        Assert.Equal(expected, connectors);
    }

    [Fact]
    public async Task Find_SearchIsSet_ReturnsFilteredList()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.Sequence.Connectors.FileSystem", "0.13.0")
        };

        var connectors = await _manager.Find("File");

        Assert.Equal(expected, connectors);
    }
}
