using System.Data;
using System.Threading;
using MELT;
using Sequence.ConnectorManagement.Base;

namespace Sequence.ConnectorManagement.Tests;

public class ConnectorRegistryTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ConnectorRegistry> _logger;
    private readonly ConnectorRegistry _registry;

    public ConnectorRegistryTests()
    {
        _loggerFactory = TestLoggerFactory.Create();
        _logger        = _loggerFactory.CreateLogger<ConnectorRegistry>();
        _registry      = new ConnectorRegistry(_logger, Helpers.IntegrationRegistrySettings);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Find_ByDefault_ReturnsLatestMajorVersionOnly()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.Sequence.Connectors.FileSystem", "0.13.0"),
            new("Reductech.Sequence.Connectors.StructuredData", "0.9.0")
        };

        var found = await _registry.Find("", false, CancellationToken.None);

        Assert.Equal(2,        found.Count);
        Assert.Equal(expected, found);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Find_WhenPrereleaseIsTrue_ReturnsAllLatestVersions()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.Sequence.Connectors.FileSystem", "0.13.0"),
            new("Reductech.Sequence.Connectors.Nuix", "0.14.0-beta.2"),
            new("Reductech.Sequence.Connectors.StructuredData", "0.9.0")
        };

        var found = await _registry.Find("", true, CancellationToken.None);

        Assert.Equal(3,     found.Count);
        Assert.Equal(found, expected);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Find_MultipleRegistriesWhenPrereleaseIsTrue_ReturnsAllLatestVersions()
    {
        var expected = new ConnectorMetadata[]
        {
            new("Reductech.Sequence.Connectors.FileSystem", "0.13.0"),
            new("Reductech.Sequence.Connectors.Nuix", "0.14.0-beta.2"),
            new("Reductech.Sequence.Connectors.StructuredData", "0.9.0")
        };

        var settings = new ConnectorManagerSettings
        {
            ConnectorPath     = @"c:\temp\connectors",
            ConfigurationPath = Helpers.ConfigurationPath,
            AutoDownload      = true,
            Registries = new ConnectorRegistryEndpoint[]
            {
                new()
                {
                    Uri =
                        "https://gitlab.com/api/v4/projects/26301248/packages/nuget/index.json"
                },
                new()
                {
                    Uri =
                        "https://gitlab.com/api/v4/projects/26301248/packages/nuget/index.json"
                }
            }
        };

        var registry = new ConnectorRegistry(_logger, settings);

        var found = await registry.Find("", true, CancellationToken.None);

        Assert.Equal(3,     found.Count);
        Assert.Equal(found, expected);
    }

    [Theory]
    [Trait("Category", "Integration")]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   true)]
    [InlineData("Reductech.Sequence.Connectors.DoesNotExist", false)]
    public async Task Exists_NoVersionSpecified(string name, bool expected)
    {
        var exists = await _registry.Exists(name);
        Assert.Equal(expected, exists);
    }

    [Theory]
    [Trait("Category", "Integration")]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   "0.13.0",                   true)]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   "0.13.0-a.main.2201311800", true)]
    [InlineData("Reductech.Sequence.Connectors.Nuix",         "0.14.0-beta.1",            true)]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   "0.8.0",                    false)]
    [InlineData("Reductech.Sequence.Connectors.DoesNotExist", "0.14.0-beta.1",            false)]
    public async Task Exists_WithVersion(string name, string version, bool expected)
    {
        var exists = await _registry.Exists(name, version);
        Assert.Equal(expected, exists);
    }

    [Theory]
    [Trait("Category", "Integration")]
    [InlineData("Reductech.Sequence.Connectors.DoesNotExist", new string[] { })]
    [InlineData("Reductech.Sequence.Connectors.FileSystem",   new[] { "0.13.0" })]
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

    [Theory]
    [Trait("Category", "Integration")]
    [InlineData("Reductech.Sequence.Connectors.DoesNotExist", new string[] { })]
    [InlineData(
        "Reductech.Sequence.Connectors.FileSystem",
        new[] { "0.13.0-a.main.2201311800", "0.13.0" }
    )]
    [InlineData(
        "Reductech.Sequence.Connectors.Nuix",
        new[] { "0.14.0-a.main.2201311748", "0.14.0-beta.1", "0.14.0-beta.2" }
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

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetConnectorPackage_WhenVersionCantBeParsed_Throws()
    {
        const string version = "Not.A.Version";

        var error = await Assert.ThrowsAsync<VersionNotFoundException>(
            () => _registry.GetConnectorPackage("DoesNotExist", version)
        );

        Assert.Equal($"Could not parse version: {version}", error.Message);
    }

    [Fact]
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

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetConnectorPackage_ReturnsConnectorPackage()
    {
        const string id      = "Reductech.Sequence.Connectors.Nuix";
        const string version = "0.14.0-beta.1";

        using var package = await _registry.GetConnectorPackage(id, version);

        Assert.Equal(id,      package.Metadata.Id);
        Assert.Equal(version, package.Metadata.Version);

        var files = await package.Package.GetFilesAsync(CancellationToken.None);

        Assert.Equal(7, files.Count());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetConnectorPackage_WhenIdIsLowerCase_ReturnsConnectorPackageWithCorrectId()
    {
        const string id              = "Reductech.Sequence.Connectors.nuix";
        const string expectedId      = "Reductech.Sequence.Connectors.Nuix";
        const string version         = "0.14.0-BETA.1";
        const string expectedVersion = "0.14.0-beta.1";

        using var package = await _registry.GetConnectorPackage(id, version);

        Assert.Equal(expectedId,      package.Metadata.Id);
        Assert.Equal(expectedVersion, package.Metadata.Version);

        var files = await package.Package.GetFilesAsync(CancellationToken.None);

        Assert.Equal(7, files.Count());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Find_WithMultipleRegistries_ReturnsPackages()
    {
        var expected = new[]
        {
            "Reductech.Sequence.Connectors.FileSystem",
            "Reductech.Sequence.Connectors.StructuredData",
            "Reductech.Sequence.Connectors.Rest" // does not exist in the default integration registry
        };

        var settings = new ConnectorManagerSettings
        {
            ConnectorPath     = @"c:\temp\connectors",
            ConfigurationPath = @"c:\temp\connectors.json",
            AutoDownload      = true,
            Registries = new ConnectorRegistryEndpoint[]
            {
                new()
                {
                    Uri =
                        "https://gitlab.com/api/v4/projects/26301248/packages/nuget/index.json"
                },
                new()
                {
                    Uri =
                        "https://gitlab.com/api/v4/projects/35096937/packages/nuget/index.json",
                    User = "conreg-integration-test",
                    // read only token for a publicly accessible registry
                    Token = "aasXXpRyJzx9jj9uKhw9"
                }
            }
        };

        var registry = new ConnectorRegistry(_logger, settings);

        var found = await registry.Find("", true, CancellationToken.None);

        foreach (var exp in expected)
            Assert.Contains(found, f => f.Id.Equals(exp));
    }
}
