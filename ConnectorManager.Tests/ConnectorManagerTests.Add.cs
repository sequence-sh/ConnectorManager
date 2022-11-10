namespace Sequence.ConnectorManagement.Tests;

public partial class ConnectorManagerTests
{
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
              && l.Message!.Equals($"Could not find connector '{id}' in the registry.")
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
                     $"Could not find connector '{id}' version '{version}' in the registry."
                 )
        );
    }

    [Fact]
    public async Task Add_WhenIdAlreadyExistsAndForceIsFalse_WritesErrorAndReturns()
    {
        const string id = "Reductech.Sequence.Connectors.StructuredData";

        await _manager.Add(id);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Connector configuration '{id}' already exists. Use --force to overwrite."
                 )
        );
    }

    [Fact]
    public async Task Add_WhenNoMajorVersionsExistAndPrereleaseIsFalse_WritesErrorAndReturns()
    {
        const string id = "Reductech.Sequence.Connectors.Nuix";

        await _manager.Add(id);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Single(log);

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals($"Could not find connector '{id}' in the registry.")
        );
    }

    [Fact]
    public async Task Add_WhenIdAlreadyExistsAndForceIsTrue_OverwritesConfiguration()
    {
        const string id = "Reductech.Sequence.Connectors.StructuredData";

        await _manager.Add(id, force: true);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Debug
              && l.Message!.Equals($"Removed '{id}' from connector configuration.")
        );

        Assert.Contains(id, _config.Keys);
    }

    [Fact]
    public async Task Add_WhenIdDoesNotExist_InstallsConnector()
    {
        const string id      = "Reductech.Sequence.Connectors.FileSystem";
        const string version = "0.13.0";

        var expected = Helpers.InstalledConnectorExpectedFiles.Select(
                f => _fileSystem.Path.Combine(
                        AppContext.BaseDirectory,
                        Helpers.InstalledConnectorPath,
                        f
                    )
                    .Replace('\\', _fileSystem.Path.DirectorySeparatorChar)
            )
            .OrderBy(f => f)
            .ToArray();

        await _manager.Add(id);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == LogLevel.Debug
              && l.Message!.Equals($"Successfully installed connector '{id}' - '{version}'.")
        );

        Assert.Equal(expected, _fileSystem.AllFiles.OrderBy(f => f).ToArray());

        Assert.Contains(id, _config.Keys);
    }

    [Fact]
    public async Task Add_WhenDirectoryAlreadyExistsAndForceIsFalse_WritesErrorAndReturns()
    {
        const string id      = "Reductech.Sequence.Connectors.FileSystem";
        const string version = "0.13.0";

        var path = _fileSystem.Path.Combine(_settings.ConnectorPath, id, version);

        _fileSystem.AddDirectory(path);

        await _manager.Add(id, version: version, force: false);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            log,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals(
                     $"Connector directory '{path}' already exists. Use --force to overwrite."
                 )
        );
    }

    [Fact]
    public async Task Add_WhenDirectoryAlreadyExistsAndForceIsTrue_InstallsConnector()
    {
        const string id      = "Reductech.Sequence.Connectors.FileSystem";
        const string version = "0.13.0";

        var expected = Helpers.InstalledConnectorExpectedFiles.Select(
                f => _fileSystem.Path.Combine(
                        AppContext.BaseDirectory,
                        Helpers.InstalledConnectorPath,
                        f
                    )
                    .Replace('\\', _fileSystem.Path.DirectorySeparatorChar)
            )
            .OrderBy(f => f)
            .ToArray();

        var path = _fileSystem.Path.Combine(_settings.ConnectorPath, id, version);

        _fileSystem.AddDirectory(path);

        await _manager.Add(id, version: version, force: true);

        var log = _loggerFactory.GetTestLoggerSink().LogEntries.ToArray();

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries,
            l => l.LogLevel == LogLevel.Debug
              && l.Message!.Equals($"Successfully installed connector '{id}' - '{version}'.")
        );

        Assert.Equal(expected, _fileSystem.AllFiles.OrderBy(f => f).ToArray());

        Assert.Contains(id, _config.Keys);
    }
}
