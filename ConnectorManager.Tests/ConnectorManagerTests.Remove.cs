using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public partial class ConnectorManagerTests
{
    [Fact]
    public async Task Remove_WhenConfigurationNotFound_WritesErrorAndReturns()
    {
        const string name = "doesnotexist";

        await _manager.Remove(name);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals($"Connector configuration '{name}' not found.")
        );
    }

    [Fact]
    public async Task Remove_WhenConnectorDirectoryNotFound_WritesWarningAndRemovesConfiguration()
    {
        const string name = "Reductech.EDR.Connectors.StructuredData";

        var expectedPath = _fileSystem.Path.Combine(
            _settings.ConnectorPath,
            _config[name].Id,
            _config[name].Version
        );

        await _manager.Remove(name);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Warning
              && l.Message!.Equals($"Connector directory '{expectedPath}' not found.")
        );

        Assert.DoesNotContain(name, _config.Keys);
    }

    [Fact]
    public async Task Remove_WhenConfigurationOnlyIsTrue_RemovesConfiguration()
    {
        const string name = "Reductech.EDR.Connectors.StructuredData";

        var path = _fileSystem.Path.Combine(
            _settings.ConnectorPath,
            _config[name].Id,
            _config[name].Version
        );

        _fileSystem.Directory.CreateDirectory(path);

        await _manager.Remove(name, true);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Debug
              && l.Message!.Equals($"Connector configuration '{name}' removed.")
        );

        Assert.DoesNotContain(name, _config.Keys);

        Assert.Contains(path, _fileSystem.AllDirectories);
    }
}

}
