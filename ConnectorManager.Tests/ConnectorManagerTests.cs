using System.IO.Abstractions.TestingHelpers;
using MELT;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public partial class ConnectorManagerTests
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
}

}
