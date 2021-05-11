using System.IO;

namespace Reductech.EDR.ConnectorManagement.Tests
{

internal static class Helpers
{
    internal static readonly ConnectorRegistrySettings IntegrationRegistrySettings = new()
    {
        Uri   = "https://gitlab.com/api/v4/projects/26301248/packages/nuget/index.json",
        User  = "integrationtests",
        Token = "E8YL7f4kTM4XJEn1ixnL"
    };

    internal const string ConfigurationPath = @"c:\temp\connectors.json";

    internal static readonly ConnectorManagerSettings ManagerSettings = new()
    {
        ConnectorPath     = @"c:\temp\connectors",
        ConfigurationPath = ConfigurationPath,
        AutoDownload      = true
    };

    internal static readonly string InstalledConnectorPath =
        @"connectors\Reductech.EDR.Connectors.FileSystem\0.9.0\Reductech.EDR.Connectors.FileSystem.dll"
            .Replace('\\', Path.DirectorySeparatorChar);

    internal const string TestConfiguration = @"
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
}

}
