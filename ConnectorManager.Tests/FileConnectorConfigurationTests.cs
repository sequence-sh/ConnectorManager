using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.ConnectorManagement.Base;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public class FileConnectorConfigurationTests
{
    private const string ConnectorName = "Reductech.EDR.Connectors.FileSystem";

    private static (IConnectorConfiguration, MockFileSystem) GetConfig(
        string file = Helpers.ConfigurationPath,
        string content = Helpers.TestConfiguration)
    {
        var fs = new MockFileSystem();
        fs.AddFile(file, content);

        var settings = file == Helpers.ConfigurationPath
            ? Helpers.ManagerSettings
            : Helpers.ManagerSettings with { ConfigurationPath = file };

        var config = new FileConnectorConfiguration(settings, fs);

        return (config, fs);
    }

    [Fact]
    public void LazyInit_WhenFileDoesNotExist_CreatesFile()
    {
        var fs     = new MockFileSystem();
        var config = new FileConnectorConfiguration(Helpers.ManagerSettings, fs);
        Assert.False(fs.FileExists(Helpers.ConfigurationPath));
        _ = config.Keys; // should initialize empty file
        Assert.Equal("{}", fs.GetFile(Helpers.ConfigurationPath).TextContents);
    }

    [Fact]
    public void Keys_ReturnsConfigurationNames()
    {
        var (config, _) = GetConfig();
        Assert.Equal(4, config.Keys.Count);
        Assert.Equal(3, config.Keys.Count(k => Regex.IsMatch(k, "^Reductech.EDR.Connectors")));
        Assert.Equal(1, config.Keys.Count(k => Regex.IsMatch(k, "disabled")));
    }

    [Fact]
    public void Settings_ReturnsSettingsObjects()
    {
        var (config, _) = GetConfig();
        Assert.Equal(4, config.Settings.Count);

        Assert.Equal(
            4,
            config.Settings.Count(s => Regex.IsMatch(s.Id, "^Reductech.EDR.Connectors"))
        );

        Assert.Equal(1, config.Settings.Count(s => !s.Enable));
    }

    [Fact]
    public void Count_ReturnsNumberOfSettings() => Assert.Equal(4, GetConfig().Item1.Count);

    [Fact]
    public async Task AddAsync_AddsConfigFile()
    {
        const string initialConfig = @"{
  ""Reductech.EDR.Connectors.StructuredData"": {
    ""id"": ""Reductech.EDR.Connectors.StructuredData"",
    ""version"": ""0.9.0""
  }
}";

        var (config, fs) = GetConfig(content: initialConfig);

        var settings = new ConnectorSettings { Id = ConnectorName, Version = "0.5.0" };
        await config.AddAsync(ConnectorName, settings, CancellationToken.None);

        Assert.Equal(2, config.Count);

        var content = fs.GetFile(Helpers.ConfigurationPath);

        Assert.NotNull(content);
        Assert.Matches(Regex.Escape(ConnectorName), content.TextContents);
    }

    [Fact]
    public async Task RemoveAsync_RemovesFromConfigFile()
    {
        var (config, fs) = GetConfig();

        await config.RemoveAsync(ConnectorName, CancellationToken.None);

        Assert.Equal(3, config.Count);
        Assert.DoesNotContain(ConnectorName, config.Keys);

        var content = fs.GetFile(Helpers.ConfigurationPath);

        Assert.NotNull(content);
        Assert.DoesNotMatch(Regex.Escape(ConnectorName), content.TextContents);
    }

    [Theory]
    [InlineData("DoesNotExist", false)]
    [InlineData(ConnectorName,  true)]
    public void Contains_ChecksKeys(string name, bool expected)
    {
        var (config, _) = GetConfig();
        Assert.Equal(expected, config.Contains(name));
    }

    [Theory]
    [InlineData("DoesNotExist",              false)]
    [InlineData("StructuredData - disabled", false)]
    [InlineData(ConnectorName,               true)]
    public void ContainsId_ChecksConnectorIds(string id, bool expected)
    {
        var (config, _) = GetConfig();
        Assert.Equal(expected, config.ContainsId(id));
    }

    [Theory]
    [InlineData("DoesNotExist",                            "0.8.0", false)]
    [InlineData(ConnectorName,                             "0.9.0", true)]
    [InlineData(ConnectorName,                             "0.5.0", false)]
    [InlineData("Reductech.EDR.Connectors.StructuredData", "0.9.0", true)]
    [InlineData("Reductech.EDR.Connectors.StructuredData", "0.8.0", true)]
    [InlineData("StructuredData - disabled",               "0.8.0", false)]
    public void ContainsVersionString_ChecksConnectorIdAndVersion(
        string id,
        string version,
        bool expected)
    {
        var (config, _) = GetConfig();
        Assert.Equal(expected, config.ContainsVersionString(id, version));
    }

    [Fact]
    public void TryGetSettings_WhenKeyExists_ReturnsTrue()
    {
        var (config, _) = GetConfig();
        var success = config.TryGetSettings(ConnectorName, out var settings);
        Assert.True(success);
        Assert.Equal(ConnectorName, settings.Id);
    }

    [Fact]
    public void TryGetSettings_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var (config, _) = GetConfig();
        var success = config.TryGetSettings("doesnotexist", out var settings);
        Assert.False(success);
        Assert.Null(settings);
    }

    [Fact]
    public void TryGetSettingsById_WhenIdExists_ReturnsAllMatchingSettings()
    {
        var (config, _) = GetConfig();

        var success = config.TryGetSettingsById(
            "Reductech.EDR.Connectors.StructuredData",
            out var settings
        );

        Assert.True(success);
        Assert.Equal(2, settings.Length);
    }

    [Fact]
    public void TryGetSettingsById_WhenIdDoesNotExist_ReturnsFalse()
    {
        var (config, _) = GetConfig();
        var success = config.TryGetSettingsById("StructuredData - disabled", out var settings);
        Assert.False(success);
        Assert.Empty(settings);
    }

    [Fact]
    public void Indexer_WhenSetting_WritesToFile()
    {
        const string expectedVersion = "0.5.0";

        var (config, fs) = GetConfig();

        var settings = new ConnectorSettings { Id = ConnectorName, Version = expectedVersion };

        config[ConnectorName] = settings;

        Assert.Equal(4, config.Count);

        var content = fs.GetFile(Helpers.ConfigurationPath);

        Assert.NotNull(content);
        Assert.Matches(Regex.Escape(expectedVersion), content.TextContents);
    }

    [Fact]
    public void Indexer_Getter_ReturnsSettings()
    {
        var (config, _) = GetConfig();
        var settings = config[ConnectorName];
        Assert.Equal(ConnectorName, settings.Id);
    }

    [Fact]
    public void Indexer_Getter_WhenKeyDoesNotExist_Throws()
    {
        const string key = "doesnotexist";

        var (config, _) = GetConfig();
        var error = Assert.Throws<KeyNotFoundException>(() => config[key]);

        Assert.Equal(
            $"The given key '{key}' was not present in the dictionary.",
            error.Message
        );
    }

    [Fact]
    public void Enumerator_Enumerates()
    {
        var (config, _) = GetConfig();

        foreach (var c in config)
            Assert.NotEmpty(c.Key);
    }
}

}
