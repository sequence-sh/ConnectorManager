using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.Internal.Errors;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

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
        await _config.RemoveAsync("Reductech.EDR.Connectors.Nuix");
        await _config.RemoveAsync("Reductech.EDR.Connectors.StructuredData");

        Assert.Empty(_config);

        var configs = _manager.List();

        Assert.Empty(configs);
    }

    [Fact]
    public void List_WhenDllDoesNotExist_WritesErrorAndContinues()
    {
        const string name    = "Reductech.EDR.Connectors.Nuix";
        const string version = "0.9.0";

        var path = _fileSystem.Path.Combine(_settings.ConnectorPath, name, version);

        var mock = new Mock<ConnectorManager>(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            _registry,
            _config,
            _fileSystem
        );

        mock.SetupSequence(m => m.LoadPlugin(It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(() => new ErrorBuilder(new Exception(), ErrorCode.Unknown))
            .Returns(() => new Result<Assembly, IErrorBuilder>());

        mock.CallBase = true;

        var configs = mock.Object.List().ToArray();

        Assert.Single(configs);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Failed to load connector configuration '{name}' from '{path}'."
                 )
        );
    }

    [Fact]
    public void List_ByDefault_ReturnsConnectorAssemblies()
    {
        const string name    = "Reductech.EDR.Connectors.Nuix";
        const string version = "0.9.0";
        const string filter  = "(?i)nuix";

        var mock = new Mock<ConnectorManager>(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            _registry,
            _config,
            _fileSystem
        );

        mock.Setup(m => m.LoadPlugin(It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(() => new Result<Assembly, IErrorBuilder>());

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
            new("Reductech.EDR.Connectors.FileSystem", "0.9.0"),
            new("Reductech.EDR.Connectors.StructuredData", "0.9.0")
        };

        var connectors = await _manager.Find();

        Assert.Equal(expected, connectors);
    }

    [Fact]
    public async Task Find_WhenPrereleaseIsTrue_ReturnsLatestVersions()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.EDR.Connectors.FileSystem", "0.9.0"),
            new("Reductech.EDR.Connectors.StructuredData", "0.9.0"),
            new("Reductech.EDR.Connectors.Nuix", "0.9.0-beta.2")
        };

        var connectors = await _manager.Find(prerelease: true);

        Assert.Equal(expected, connectors);
    }

    [Fact]
    public async Task Find_SearchIsSet_ReturnsFilteredList()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.EDR.Connectors.FileSystem", "0.9.0")
        };

        var connectors = await _manager.Find("File");

        Assert.Equal(expected, connectors);
    }
}

}
