using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Xunit;

namespace Reductech.EDR.ConnectorManagement.Tests
{

public class FileConnectorConfigurationTests
{
    private const string FilePath = @"c:\temp\connectors.json";
    private const string ConnectorName = "Reductech.EDR.Connectors.FileSystem";

    private const string TestConfiguration = @"
{
  ""Reductech.EDR.Connectors.Nuix"": {
    ""id"": ""Reductech.EDR.Connectors.Nuix"",
    ""version"": ""0.9.0"",
    ""enabled"": true,
    ""settings"": {
      ""exeConsolePath"": ""C:\\Program Files\\Nuix\\Nuix 9.0\\nuix_console.exe"",
      ""licencesourcetype"": ""dongle"",
      ""version"": ""9.0"",
      ""features"": [
        ""ANALYSIS"",
        ""CASE_CREATION""
      ]
    }
  },
  ""Reductech.EDR.Connectors.FileSystem"": {
    ""id"": ""Reductech.EDR.Connectors.FileSystem"",
    ""version"": ""0.9.0""
  },
  ""Reductech.EDR.Connectors.StructuredData"": {
    ""id"": ""Reductech.EDR.Connectors.StructuredData"",
    ""version"": ""0.9.0""
  },
  ""StructuredData - disabled"": {
    ""id"": ""Reductech.EDR.Connectors.StructuredData"",
    ""version"": ""0.8.0"",
    ""enable"": false
  }
}";

    private static async Task<(IConnectorConfiguration, MockFileSystem)> GetEmptyConfig() =>
        await GetConfig(content: "");

    private static async Task<(IConnectorConfiguration, MockFileSystem)> GetConfig(
        string file = FilePath,
        string content = TestConfiguration)
    {
        var fs = new MockFileSystem();
        fs.AddFile(file, content);
        var config = await FileConnectorConfiguration.FromJson(FilePath, fs);
        return (config, fs);
    }

    [Fact]
    public async Task FromJson_WhenFileDoesNotExist_Throws()
    {
        var error = await Assert.ThrowsAsync<FileNotFoundException>(
            () => FileConnectorConfiguration.FromJson("doesnotexist", new MockFileSystem())
        );

        Assert.Equal("Connector configuration file not found.", error.Message);
    }

    [Fact]
    public async Task FromJson_WhenConfigFileIsValid_ReturnsConfig()
    {
        var (config, _) = await GetConfig();

        var nuixSettings = config["Reductech.EDR.Connectors.Nuix"];

        Assert.Equal(4, config.Count);
        Assert.IsType<Entity>(nuixSettings.Settings);

        var features = nuixSettings.Settings.TryGetNestedList("features");

        Assert.True(features.HasValue);
        Assert.Equal(2, features.Value.Length);
        Assert.Contains("ANALYSIS", features.Value);
    }

    [Fact]
    public async Task FromJson_WhenConfigFileIsNotValid_Throws()
    {
        var fs = new MockFileSystem();
        fs.AddFile(FilePath, "{\"notright:\"\"}");

        var error = await Assert.ThrowsAsync<JsonReaderException>(
            () => FileConnectorConfiguration.FromJson(FilePath, fs)
        );

        Assert.Matches("Invalid character", error.Message);
    }

    [Fact]
    public async Task FromJson_WhenConfigIsEmpty_ReturnsEmptyConfig()
    {
        var (config, _) = await GetEmptyConfig();
        Assert.Empty(config);
    }

    [Fact]
    public async Task Keys_ReturnsConfigurationNames()
    {
        var (config, _) = await GetConfig();
        Assert.Equal(4, config.Keys.Count);
        Assert.Equal(3, config.Keys.Count(k => Regex.IsMatch(k, "^Reductech.EDR.Connectors")));
        Assert.Equal(1, config.Keys.Count(k => Regex.IsMatch(k, "disabled")));
    }

    [Fact]
    public async Task Settings_ReturnsSettingsObjects()
    {
        var (config, _) = await GetConfig();
        Assert.Equal(4, config.Settings.Count);

        Assert.Equal(
            4,
            config.Settings.Count(s => Regex.IsMatch(s.Id, "^Reductech.EDR.Connectors"))
        );

        Assert.Equal(1, config.Settings.Count(s => !s.Enable));
    }

    [Fact]
    public async Task Count_ReturnsNumberOfSettings() =>
        Assert.Equal(4, (await GetConfig()).Item1.Count);

    [Fact]
    public async Task AddAsync_AddsConfigFile()
    {
        const string initialConfig = @"{
  ""Reductech.EDR.Connectors.StructuredData"": {
    ""id"": ""Reductech.EDR.Connectors.StructuredData"",
    ""version"": ""0.9.0""
  }
}";

        var (config, fs) = await GetConfig(content: initialConfig);

        var settings = new ConnectorSettings { Id = ConnectorName, Version = "0.5.0" };
        await config.AddAsync(ConnectorName, settings, CancellationToken.None);

        Assert.Equal(2, config.Count);

        var content = fs.GetFile(FilePath);

        Assert.NotNull(content);
        Assert.Matches(Regex.Escape(ConnectorName), content.TextContents);
    }

    [Fact]
    public async Task RemoveAsync_RemovesFromConfigFile()
    {
        var (config, fs) = await GetConfig();

        await config.RemoveAsync(ConnectorName, CancellationToken.None);

        Assert.Equal(3, config.Count);
        Assert.DoesNotContain(ConnectorName, config.Keys);

        var content = fs.GetFile(FilePath);

        Assert.NotNull(content);
        Assert.DoesNotMatch(Regex.Escape(ConnectorName), content.TextContents);
    }

    [Theory]
    [InlineData("DoesNotExist", false)]
    [InlineData(ConnectorName,  true)]
    public async Task Contains_ChecksKeys(string name, bool expected)
    {
        var (config, _) = await GetConfig();
        Assert.Equal(expected, config.Contains(name));
    }

    [Theory]
    [InlineData("DoesNotExist",              false)]
    [InlineData("StructuredData - disabled", false)]
    [InlineData(ConnectorName,               true)]
    public async Task ContainsId_ChecksConnectorIds(string id, bool expected)
    {
        var (config, _) = await GetConfig();
        Assert.Equal(expected, config.ContainsId(id));
    }

    [Theory]
    [InlineData("DoesNotExist",                            "0.8.0", false)]
    [InlineData(ConnectorName,                             "0.9.0", true)]
    [InlineData(ConnectorName,                             "0.5.0", false)]
    [InlineData("Reductech.EDR.Connectors.StructuredData", "0.9.0", true)]
    [InlineData("Reductech.EDR.Connectors.StructuredData", "0.8.0", true)]
    [InlineData("StructuredData - disabled",               "0.8.0", false)]
    public async Task ContainsVersionString_ChecksConnectorIdAndVersion(
        string id,
        string version,
        bool expected)
    {
        var (config, fs) = await GetConfig();
        Assert.Equal(expected, config.ContainsVersionString(id, version));
    }

    [Fact]
    public async Task TryGetSettings_WhenKeyExists_ReturnsTrue()
    {
        var (config, _) = await GetConfig();
        var success = config.TryGetSettings(ConnectorName, out var settings);
        Assert.True(success);
        Assert.Equal(ConnectorName, settings.Id);
    }

    [Fact]
    public async Task TryGetSettings_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var (config, _) = await GetConfig();
        var success = config.TryGetSettings("doesnotexist", out var settings);
        Assert.False(success);
        Assert.Null(settings);
    }

    [Fact]
    public async Task TryGetSettingsById_WhenIdExists_ReturnsAllMatchingSettings()
    {
        var (config, _) = await GetConfig();

        var success = config.TryGetSettingsById(
            "Reductech.EDR.Connectors.StructuredData",
            out var settings
        );

        Assert.True(success);
        Assert.Equal(2, settings.Length);
    }

    [Fact]
    public async Task TryGetSettingsById_WhenIdDoesNotExist_ReturnsFalse()
    {
        var (config, _) = await GetConfig();
        var success = config.TryGetSettingsById("StructuredData - disabled", out var settings);
        Assert.False(success);
        Assert.Empty(settings);
    }

    [Fact]
    public async Task Indexer_WhenSetting_WritesToFile()
    {
        const string expectedVersion = "0.5.0";

        var (config, fs) = await GetConfig();

        var settings = new ConnectorSettings { Id = ConnectorName, Version = expectedVersion };

        config[ConnectorName] = settings;

        Assert.Equal(4, config.Count);

        var content = fs.GetFile(FilePath);

        Assert.NotNull(content);
        Assert.Matches(Regex.Escape(expectedVersion), content.TextContents);
    }

    [Fact]
    public async Task Indexer_Getter_ReturnsSettings()
    {
        var (config, _) = await GetConfig();
        var settings = config[ConnectorName];
        Assert.Equal(ConnectorName, settings.Id);
    }

    [Fact]
    public async Task Indexer_Getter_WhenKeyDoesNotExist_Throws()
    {
        const string key = "doesnotexist";

        var (config, _) = await GetConfig();
        var error = Assert.Throws<KeyNotFoundException>(() => config[key]);

        Assert.Equal(
            $"The given key '{key}' was not present in the dictionary.",
            error.Message
        );
    }

    [Fact]
    public async Task Enumerator_Enumerates()
    {
        var (config, _) = await GetConfig();

        foreach (var c in config)
            Assert.NotEmpty(c.Key);
    }
}

}
