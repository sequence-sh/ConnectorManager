using System.Collections.Generic;
using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.ConnectorManagement.Tests;

internal static class Helpers
{
    internal static readonly ConnectorRegistrySettings IntegrationRegistrySettings = new()
    {
        Uri = "https://gitlab.com/api/v4/projects/26301248/packages/nuget/index.json"
    };

    internal const string ConfigurationPath = @"c:\temp\connectors.json";

    internal static readonly ConnectorManagerSettings ManagerSettings = new()
    {
        ConnectorPath     = @"c:\temp\connectors",
        ConfigurationPath = ConfigurationPath,
        AutoDownload      = true
    };

    internal static readonly string InstalledConnectorPath =
        @"connectors\Reductech.Sequence.Connectors.FileSystem\0.9.0";

    internal static readonly string[] InstalledConnectorExpectedFiles =
    {
        "x64\\additional.dll", "content.txt", "Reductech.Sequence.Connectors.FileSystem.dll",
        "Reductech.Sequence.Connectors.FileSystem.xml", "System.IO.Abstractions.dll"
    };

    internal const string TestConfiguration = @"
{
  ""Reductech.Sequence.Connectors.Nuix"": {
    ""id"": ""Reductech.Sequence.Connectors.Nuix"",
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
  ""Reductech.Sequence.Connectors.FileSystem"": {
    ""id"": ""Reductech.Sequence.Connectors.FileSystem"",
    ""version"": ""0.9.0""
  },
  ""Reductech.Sequence.Connectors.StructuredData"": {
    ""id"": ""Reductech.Sequence.Connectors.StructuredData"",
    ""version"": ""0.9.0""
  },
  ""StructuredData - disabled"": {
    ""id"": ""Reductech.Sequence.Connectors.StructuredData"",
    ""version"": ""0.8.0"",
    ""enable"": false
  }
}";

    internal static Dictionary<string, ConnectorSettings> GetDefaultConnectors() => new()
    {
        {
            "Reductech.Sequence.Connectors.Nuix",
            new ConnectorSettings { Id = "Reductech.Sequence.Connectors.Nuix", Version = "0.9.0" }
        },
        {
            "Reductech.Sequence.Connectors.StructuredData",
            new ConnectorSettings
            {
                Id = "Reductech.Sequence.Connectors.StructuredData", Version = "0.8.0"
            }
        }
    };
}
