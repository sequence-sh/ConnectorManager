using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using MELT;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public class ConnectorManagerTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConnectorConfiguration _config;
    private readonly ConnectorManagerSettings _settings;
    private readonly MockFileSystem _fileSystem;
    private readonly ConnectorManager _manager;

    public ConnectorManagerTests()
    {
        _loggerFactory = TestLoggerFactory.Create();
        _config        = FakeConnectorConfiguration.GetDefaultConfiguration();
        _settings      = ConnectorManagerSettings.Default;
        _fileSystem    = new MockFileSystem();

        _manager = new ConnectorManager(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            new FakeConnectorRegistry(),
            _config,
            _fileSystem
        );
    }

    [Fact]
    public async Task Add_WhenConnectorDoesNotExist_WritesErrorAndReturns()
    {
        const string? id = "doesnotexist";

        await _manager.Add(id);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals($"Could not find connector {id} in the registry.")
        );
    }

    [Fact]
    public async Task Add_WhenVersionDoesNotExist_WritesErrorAndReturns()
    {
        const string version = "0.1.0";

        var id = FakeConnectorRegistry.Connectors[0].Id;

        await _manager.Add(id, version: version);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Could not find connector {id} version {version} in the registry."
                 )
        );
    }

    [Fact]
    public async Task Add_WhenIdAlreadyExistsAndForceIsFalse_WritesErrorAndReturns()
    {
        const string id = "Reductech.EDR.Connectors.StructuredData";

        await _manager.Add(id);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Connector configuration already exists {id}. Use --force to overwrite or update."
                 )
        );
    }

    [Fact]
    public async Task Add_WhenNoMajorVersionsExistAndPrereleaseIsFalse_WritesErrorAndReturns()
    {
        const string id = "Reductech.EDR.Connectors.Nuix";

        await _manager.Add(id);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals($"Could not find connector {id} in the registry.")
        );
    }

    [Fact]
    public async Task Add_WhenIdAlreadyExistsAndForceIsTrue_OverwritesConfiguration()
    {
        const string id = "Reductech.EDR.Connectors.StructuredData";

        await _manager.Add(id, force: true);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Equal(2, log.Length);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Debug
              && l.Message!.Equals($"Removed {id} from connector configuration.")
        );

        Assert.Contains(id, _config.Keys);
    }

    [Fact]
    public async Task Add_WhenIdDoesNotExist_InstallsConnector()
    {
        const string id      = "Reductech.EDR.Connectors.FileSystem";
        const string version = "0.9.0";

        var expected = new[]
        {
            _fileSystem.Path.Combine(
                AppContext.BaseDirectory,
                @"connectors\Reductech.EDR.Connectors.FileSystem\0.9.0\Reductech.EDR.Connectors.FileSystem.dll"
            )
        };

        await _manager.Add(id);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == LogLevel.Information
              && l.Message!.Equals($"Successfully installed connector {id} ({version}).")
        );

        Assert.Equal(expected, _fileSystem.AllFiles);

        Assert.Contains(id, _config.Keys);
    }

    [Fact]
    public async Task Add_WhenDirectoryAlreadyExistsAndForceIsFalse_WritesErrorAndReturns()
    {
        const string id      = "Reductech.EDR.Connectors.FileSystem";
        const string version = "0.9.0";

        var path = _fileSystem.Path.Combine(_settings.ConnectorPath, id, version);

        _fileSystem.AddDirectory(path);

        await _manager.Add(id, version: version, force: false);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Connector directory {path} already exists. Use --force to overwrite."
                 )
        );
    }

    [Fact]
    public async Task Add_WhenDirectoryAlreadyExistsAndForceIsTrue_InstallsConnector()
    {
        const string id      = "Reductech.EDR.Connectors.FileSystem";
        const string version = "0.9.0";

        var expected = new[]
        {
            _fileSystem.Path.Combine(
                AppContext.BaseDirectory,
                @"connectors\Reductech.EDR.Connectors.FileSystem\0.9.0\Reductech.EDR.Connectors.FileSystem.dll"
            )
        };

        var path = _fileSystem.Path.Combine(_settings.ConnectorPath, id, version);

        _fileSystem.AddDirectory(path);

        await _manager.Add(id, version: version, force: true);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries,
            l => l.LogLevel == LogLevel.Information
              && l.Message!.Equals($"Successfully installed connector {id} ({version}).")
        );

        Assert.Equal(expected, _fileSystem.AllFiles);

        Assert.Contains(id, _config.Keys);
    }
}

}
