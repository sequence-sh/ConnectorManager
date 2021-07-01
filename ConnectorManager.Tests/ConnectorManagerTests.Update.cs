using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public partial class ConnectorManagerTests
{
    [Fact]
    public async Task Update_WhenConfigurationDoesNotExist_WritesErrorAndReturns()
    {
        const string name = "doesnotexist";

        await _manager.Update(name);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Connector configuration '{name}' does not exist. To install, use add."
                 )
        );
    }

    [Fact]
    public async Task Update_WhenVersionIsSameAsConfig_WritesInfoAndReturns()
    {
        const string name    = "Reductech.EDR.Connectors.Nuix";
        const string version = "0.9.0";

        await _manager.Update(name, version);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Information
              && l.Message!.Equals(
                     $"Connector configuration '{name}' already has version '{version}'."
                 )
        );
    }

    [Fact]
    public async Task Update_WhenConnectorDoesNotExist_WritesErrorAndReturns()
    {
        const string name = "Reductech.EDR.Connectors.Nuix";
        const string id   = "doesnotexist";

        _config[name].Id = id;

        await _manager.Update(name);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals($"Could not find connector '{id}' in the registry.")
        );
    }

    [Fact]
    public async Task Update_WhenVersionIsAlreadyLatest_WritesErrorAndReturns()
    {
        const string name    = "Reductech.EDR.Connectors.StructuredData";
        const string version = "0.9.0";

        _config[name].Version = version;

        await _manager.Update(name);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Information
              && l.Message!.Equals(
                     $"Connector configuration '{name}' already has the latest version '{version}' installed."
                 )
        );
    }

    [Fact]
    public async Task Update_WhenInstallFails_WritesErrorAndReturns()
    {
        const string name    = "Reductech.EDR.Connectors.Nuix";
        const string version = "0.9.0-a.master.2105052200";

        var path = _fileSystem.Path.Combine(_settings.ConnectorPath, name, version);

        _fileSystem.AddDirectory(path);

        await _manager.Update(name, version, true);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Connector directory '{path}' already exists. Use --force to overwrite."
                 )
        );
    }

    [Fact]
    public async Task Update_ByDefault_InstallsConnectorAndUpdatesVersion()
    {
        const string name    = "Reductech.EDR.Connectors.Nuix";
        const string version = "0.9.0-a.master.2105052200";

        var expected = _fileSystem.Path.Combine(
            AppContext.BaseDirectory,
            $@"connectors\{name}\{version}\Reductech.EDR.Connectors.FileSystem.dll".Replace(
                '\\',
                _fileSystem.Path.DirectorySeparatorChar
            )
        );

        await _manager.Update(name, version, true);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == LogLevel.Information
              && l.Message!.Equals(
                     $"Connector configuration '{name}' successfully updated to '{version}'."
                 )
        );

        Assert.Contains(expected, _fileSystem.AllFiles);

        Assert.Equal(version, _config[name].Version);
    }

    [Fact]
    public async Task Update_WhenVersionIsNull_UpdatesToLatestVersion()
    {
        const string name    = "Reductech.EDR.Connectors.StructuredData";
        const string version = "0.9.0";

        var expected = _fileSystem.Path.Combine(
            AppContext.BaseDirectory,
            $@"connectors\{name}\{version}\Reductech.EDR.Connectors.FileSystem.dll".Replace(
                '\\',
                _fileSystem.Path.DirectorySeparatorChar
            )
        );

        await _manager.Update(name);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == LogLevel.Information
              && l.Message!.Equals(
                     $"Connector configuration '{name}' successfully updated to '{version}'."
                 )
        );

        Assert.Contains(expected, _fileSystem.AllFiles);

        Assert.Equal(version, _config[name].Version);
    }
}

}
