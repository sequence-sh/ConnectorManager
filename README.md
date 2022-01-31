# Sequence® Connector Manager

A plugins management system for [Sequence Connectors](https://gitlab.com/reductech/sequence/connectors).

# Connectors

All the available connectors can be found in the
[Connector Registry](https://gitlab.com/reductech/sequence/connector-registry/-/packages).
The connector manager is set up to work with this registry by default.

# Example `connectors.json`

```json
{
  "Reductech.Sequence.Connectors.Nuix": {
    "id": "Reductech.Sequence.Connectors.Nuix",
    "version": "0.13.0",
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
  "Reductech.Sequence.Connectors.FileSystem": {
    "id": "Reductech.Sequence.Connectors.FileSystem",
    "version": "0.13.0"
  },
  "data": {
    "id": "Reductech.Sequence.Connectors.StructuredData",
    "version": "0.13.0",
    "enabled": false
  }
}
```

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

## Connector Directory Structure

Everything in these nuget package directories will be extracted to the connector's
root directory, preserving any subdirectories:

- `lib/net6.0/`
- `contentFiles/any/any/`

All other files will be extracted to their respective paths.

# Testing

`ConnectorRegistry` is integration tested against the
[nuget feed](https://gitlab.com/reductech/sequence/connectormanager/-/packages) of this project.

# Releases

Can be downloaded from the [Releases page](https://gitlab.com/reductech/sequence/connectormanager/-/releases).

# NuGet Packages

Are available in the [Reductech Nuget feed](https://gitlab.com/reductech/nuget/-/packages).
