using System.Data;
using System.Threading;
using MELT;
using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.ConnectorManagement.Tests;

public class ConnectorRegistryTests
{
    public const string Skip = "Temporary";

    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ConnectorRegistry> _logger;
    private readonly ConnectorRegistry _registry;

    public ConnectorRegistryTests()
    {
        _loggerFactory = TestLoggerFactory.Create();
        _logger        = _loggerFactory.CreateLogger<ConnectorRegistry>();
        _registry      = new ConnectorRegistry(_logger, Helpers.IntegrationRegistrySettings);
    }

    [Fact(Skip = Skip)]
    [Trait("Category", "Integration")]
    public async Task Find_ByDefault_ReturnsLatestMajorVersionOnly()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.Sequence.Connectors.FileSystem", "0.9.0"),
            new("Reductech.Sequence.Connectors.StructuredData", "0.9.0")
        };

        var found = await _registry.Find("", false, CancellationToken.None);

        Assert.Equal(2,        found.Count);
        Assert.Equal(expected, found);
    }

    [Fact(Skip = Skip)]
    [Trait("Category", "Integration")]
    public async Task Find_WhenPrereleaseIsTrue_ReturnsAllLatestVersions()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.Sequence.Connectors.FileSystem", "0.9.0"),
            new("Reductech.Sequence.Connectors.Nuix", "0.9.0-beta.2"),
            new("Reductech.Sequence.Connectors.StructuredData", "0.9.0")
        };

        var found = await _registry.Find("", true, CancellationToken.None);

        Assert.Equal(3,     found.Count);
        Assert.Equal(found, expected);
    }

    [Theory(Skip = Skip)]
    [Trait("Category", "Integration")]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   true)]
    [InlineData("Reductech.Sequence.Connectors.DoesNotExist", false)]
    public async Task Exists_NoVersionSpecified(string name, bool expected)
    {
        var exists = await _registry.Exists(name);
        Assert.Equal(expected, exists);
    }

    [Theory(Skip = Skip)]
    [Trait("Category", "Integration")]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   "0.9.0",                     true)]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   "0.9.0-a.master.2105052158", true)]
    [InlineData("Reductech.Sequence.Connectors.Nuix",         "0.9.0-beta.1",              true)]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   "0.8.0",                     false)]
    [InlineData("Reductech.Sequence.Connectors.DoesNotExist", "0.9.0-beta.1",              false)]
    public async Task Exists_WithVersion(string name, string version, bool expected)
    {
        var exists = await _registry.Exists(name, version);
        Assert.Equal(expected, exists);
    }

    [Theory(Skip = Skip)]
    [Trait("Category", "Integration")]
    [InlineData("Reductech.Sequence.Connectors.DoesNotExist", new string[] { })]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   new[] { "0.9.0" })]
    [InlineData("Reductech.Sequence.Connectors.Nuix",         new string[] { })]
    [InlineData(
        "Reductech.Sequence.Connectors.StructuredData",
        new[] { "0.7.0", "0.8.0", "0.9.0" }
    )]
    public async Task GetVersion_WhenPrereleaseIsFalse_ReturnsOnlyMajorVersion(
        string name,
        string[] expected)
    {
        var exists = await _registry.GetVersion(name);
        Assert.Equal(expected, exists);
    }

    [Theory(Skip = Skip)]
    [Trait("Category", "Integration")]
    [InlineData("Reductech.Sequence.Connectors.DoesNotExist", new string[] { })]
    [InlineData(
        "Reductech.Sequence.Connectors.FileSystem",
        new[] { "0.9.0-a.master.2105052158", "0.9.0" }
    )]
    [InlineData(
        "Reductech.Sequence.Connectors.Nuix",
        new[] { "0.9.0-a.master.2105052200", "0.9.0-beta.1", "0.9.0-beta.2" }
    )]
    [InlineData(
        "Reductech.Sequence.Connectors.StructuredData",
        new[] { "0.7.0", "0.8.0", "0.9.0" }
    )]
    public async Task GetVersion_WhenPrereleaseIsTrue_ReturnsMajorAndPrereleaseVersions(
        string name,
        string[] expected)
    {
        var exists = await _registry.GetVersion(name, true);
        Assert.Equal(expected, exists);
    }

    [Fact(Skip = Skip)]
    [Trait("Category", "Integration")]
    public async Task GetConnectorPackage_WhenVersionCantBeParsed_Throws()
    {
        const string version = "Not.A.Version";

        var error = await Assert.ThrowsAsync<VersionNotFoundException>(
            () => _registry.GetConnectorPackage("DoesNotExist", version)
        );

        Assert.Equal($"Could not parse version: {version}", error.Message);
    }

    [Fact(Skip = Skip)]
    [Trait("Category", "Integration")]
    public async Task GetConnectorPackage_WhenConnectorNotFound_Throws()
    {
        const string id      = "Reductech.Sequence.Connectors.Nuix";
        const string version = "0.5.0";

        var error = await Assert.ThrowsAsync<ArgumentException>(
            () => _registry.GetConnectorPackage(id, version)
        );

        Assert.Equal($"Can't find connector {id} ({version})", error.Message);
    }

    [Fact(Skip = Skip)]
    [Trait("Category", "Integration")]
    public async Task GetConnectorPackage_ReturnsConnectorPackage()
    {
        const string id      = "Reductech.Sequence.Connectors.Nuix";
        const string version = "0.9.0-beta.1";

        using var package = await _registry.GetConnectorPackage(id, version);

        Assert.Equal(id,      package.Metadata.Id);
        Assert.Equal(version, package.Metadata.Version);

        var files = await package.Package.GetFilesAsync(CancellationToken.None);

        Assert.Equal(14, files.Count());
    }

    [Fact(Skip = Skip)]
    [Trait("Category", "Integration")]
    public async Task GetConnectorPackage_WhenIdIsLowerCase_ReturnsConnectorPackageWithCorrectId()
    {
        const string id              = "Reductech.Sequence.Connectors.nuix";
        const string expectedId      = "Reductech.Sequence.Connectors.Nuix";
        const string version         = "0.9.0-BETA.1";
        const string expectedVersion = "0.9.0-beta.1";

        using var package = await _registry.GetConnectorPackage(id, version);

        Assert.Equal(expectedId,      package.Metadata.Id);
        Assert.Equal(expectedVersion, package.Metadata.Version);

        var files = await package.Package.GetFilesAsync(CancellationToken.None);

        Assert.Equal(14, files.Count());
    }
}
