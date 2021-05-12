using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public class ConnectorManagerExtensionsTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConnectorConfiguration _config;
    private readonly ConnectorManagerSettings _settings;
    private readonly MockFileSystem _fileSystem;
    private readonly IConnectorRegistry _registry;
    private readonly ConnectorManager _manager;

    public ConnectorManagerExtensionsTests()
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
    public async Task GetStepFactoryStoreAsync_ReturnsStepFactory()
    {
        var sfs = await _manager.GetStepFactoryStoreAsync();
        Assert.NotNull(sfs);
    }

    [Fact]
    public async Task GetStepFactoryStoreAsync_WhenValidationFails_Throws()
    {
        var manager = new ConnectorManager(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings with { AutoDownload = false },
            _registry,
            _config,
            _fileSystem
        );

        var error =
            await Assert.ThrowsAsync<ConnectorManagerExtensions.ConnectorConfigurationException>(
                () => manager.GetStepFactoryStoreAsync()
            );

        Assert.Equal("Could not validate installed connectors.", error.Message);
    }

    [Fact]
    public async Task GetStepFactoryStoreAsync_WhenSame_Throws()
    {
        await _config.AddAsync(
            "Reductech.EDR.Connectors.StructuredData -old",
            new ConnectorSettings
            {
                Id = "Reductech.EDR.Connectors.StructuredData", Version = "0.8.0", Enable = true
            }
        );

        var mock = new Mock<ConnectorManager>(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            _registry,
            _config,
            _fileSystem
        );

        mock.Setup(m => m.LoadPlugin(It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(() => new Result<Assembly, IErrorBuilder>());

        var error =
            await Assert.ThrowsAsync<ConnectorManagerExtensions.ConnectorConfigurationException>(
                () => mock.Object.GetStepFactoryStoreAsync()
            );

        Assert.Equal(
            "When using multiple configurations with the same connector id, at most one can be enabled.",
            error.Message
        );
    }
}

}
