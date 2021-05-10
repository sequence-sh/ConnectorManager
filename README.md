# Connector Manager

A plugins management system for [EDR Connectors](https://gitlab.com/reductech/edr/connectors).

# Connectors

All the available connectors can be found in the
[Connector Registry](https://gitlab.com/reductech/edr/connector-registry/-/packages).
The connector manager is set up to work with this registry by default.

# Example `connectors.json`

```json
{
  "Reductech.EDR.Connectors.Nuix": {
    "id": "Reductech.EDR.Connectors.Nuix",
    "version": "0.9.0",
    "settings": {
      "exeConsolePath": "C:\\Program Files\\Nuix\\Nuix 9.0\\nuix_console.exe",
      "licencesourcetype": "dongle",
      "version": "9.0",
      "features": [
        "ANALYSIS",
        "CASE_CREATION",
        "EXPORT_ITEMS",
        "METADATA_IMPORT",
        "OCR_PROCESSING",
        "PRODUCTION_SET"
      ]
    }
  },
  "Reductech.EDR.Connectors.FileSystem": {
    "id": "Reductech.EDR.Connectors.FileSystem",
    "version": "0.9.0"
  },
  "data": {
    "id": "Reductech.EDR.Connectors.StructuredData",
    "version": "0.9.0",
    "enabled": false
  }
}
```

# E-Discovery Reduct

The Connector Manager is part of a group of projects called
[E-Discovery Reduct](https://gitlab.com/reductech/edr)
which consists of a collection of [Connectors](https://gitlab.com/reductech/edr/connectors)
and a command-line application for running Sequences, called
[EDR](https://gitlab.com/reductech/edr/edr/-/releases).

You can see an implementation of the Connector Manager for the console
in [EDR](https://gitlab.com/reductech/edr/edr/-/releases).

# Packaging Projects as Connectors

For a nuget package to be compatible with Core, all the dependencies
need to be included. To do this, add this to your `csproj` file:

```xml
  <PropertyGroup Condition="$(PackConnector) != ''">
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>

  <Target Name="AddConnectorDependencies"
          BeforeTargets="GenerateNuspec"
          Condition="$(PackConnector) != ''">
    <ItemGroup>
      <_PackageFiles
        Include="@(RuntimeCopyLocalItems)"
        PackagePath="$(BuildOutputTargetFolder)/$(TargetFramework)/%(Filename)%(Extension)" />
    </ItemGroup>
  </Target>
```

Then package the connector using:

```powershell
dotnet pack --configuration Release --version-suffix "-alpha.1" -p:PackConnector=true --output ./
```

# Testing

`ConnectorRegistry` is integration tested against the
[nuget feed](https://gitlab.com/reductech/edr/connectormanager/-/packages) of this project.

# Releases

Can be downloaded from the [Releases page](https://gitlab.com/reductech/edr/connectormanager/-/releases).

# NuGet Packages

Are available in the [Reductech Nuget feed](https://gitlab.com/reductech/nuget/-/packages).
