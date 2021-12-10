using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests;

public partial class ConnectorManagerTests
{
    [Fact]
    public async Task Verify_WhenDirectoryAndDllExist_ReturnsTrue()
    {
        foreach (var c in _config.Settings)
        {
            var dir = _fileSystem.Path.Combine(_settings.ConnectorPath, c.Id, c.Version);
            _fileSystem.AddDirectory(dir);
            var dll = _fileSystem.Path.Combine(dir, $"{c.Id}.dll");
            _fileSystem.AddFile(dll, "");
        }

        var result = await _manager.Verify();

        Assert.True(result);
    }

    [Fact]
    public async Task Verify_WhenOnlyDirectoryExists_ReturnsFalse()
    {
        foreach (var c in _config.Settings)
        {
            var dir = _fileSystem.Path.Combine(_settings.ConnectorPath, c.Id, c.Version);

            _fileSystem.AddDirectory(dir);

            if (c.Id.Equals("Reductech.EDR.Connectors.StructuredData"))
            {
                var dll = _fileSystem.Path.Combine(dir, $"{c.Id}.dll");
                _fileSystem.AddFile(dll, "");
            }
        }

        var result = await _manager.Verify();

        Assert.False(result);
    }

    [Fact]
    public async Task Verify_WhenDirectoryDoesNotExistAndAutoDownloadIsFalse_ReturnsFalse()
    {
        var exists = _config["Reductech.EDR.Connectors.StructuredData"];
        var dir    = _fileSystem.Path.Combine(_settings.ConnectorPath, exists.Id, exists.Version);
        _fileSystem.AddDirectory(dir);
        var dll = _fileSystem.Path.Combine(dir, $"{exists.Id}.dll");
        _fileSystem.AddFile(dll, "");

        var missing = _config["Reductech.EDR.Connectors.Nuix"];

        var missingDir = _fileSystem.Path.Combine(
            _settings.ConnectorPath,
            missing.Id,
            missing.Version
        );

        var manager = new ConnectorManager(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings with { AutoDownload = false },
            _registry,
            _config,
            _fileSystem
        );

        var result = await manager.Verify();

        Assert.False(result);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Configuration '{missing.Id}' installation path missing: {missingDir}"
                 )
        );
    }

    [Fact]
    public async Task
        Verify_WhenDirectoryDoesNotExistAndInstallMissingIsTrue_InstallsConnectorAndReturnsTrue()
    {
        var result = await _manager.Verify();

        Assert.True(result);

        foreach (var c in _config.Settings)
        {
            var dll = _fileSystem.Path.Combine(
                _settings.ConnectorPath,
                c.Id,
                c.Version,
                "Reductech.EDR.Connectors.FileSystem.dll"
            );

            Assert.Contains(dll, _fileSystem.AllFiles);
        }
    }
}
