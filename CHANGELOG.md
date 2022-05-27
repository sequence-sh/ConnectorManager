# v0.8.0 (2022-05-27)

## Summary of Changes


- Consolidated connector registry and manager settings
- The connector manager `appsettings.json` key has changed from `sequence` to `connectorManager`
- `ConnectorRegistrySettings` type has been removed
- It's now possible to specify more than one registry in the configuration:

```json
"connectorManager": {
  "connectorPath": "c:\\connectors",
  "configurationPath": "c:\\connectors\\connectors.json",
  "autoDownload": false,
  "registries": [
      {
          "uri": "https://registry/packages/index.json"
      },
      {
        "uri": "https://another-registry/packages/index.json",
        "User": "ci-login-token",
        "Token": "abc123"
      }
  ],
  "enableNuGetLog": true
}
```

## Issues Closed in this Release

### New Features

- Allow setting multiple connector registry endpoints #41

### Bug Fixes

- Using the prerelease flag returns multiple versions for a connector #43

# v0.7.0 (2022-03-25)

Bug fix and maintenance release.

## Issues Closed in this Release

### Bug Fixes

- Dependency resolution fails on case-sensitive file systems #28

### Maintenance

- ReEnable tests #32

# v0.6.0 (2022-01-16)

EDR is now Sequence. The following has changed:

- The GitLab group has moved to https://gitlab.com/reductech/sequence
- The root namespace is now `Reductech.Sequence`
- The documentation site has moved to https://sequence.sh

Everything else is still the same - automation, simplified.

The project has now been updated to use .NET 6.

## Issues Closed in this Release

### Bug Fixes

- Connector manager should flatten paths when installing #30

### Maintenance

- Rename EDR to Sequence #31
- Use dotnet 6 #29
- Add all connectors if connector data is null #27

# v0.5.1 (2021-11-29)

Bug fix release.

## Issues Closed in this Release

### Bug Fixes

- ConnectorManager should not load the same Assembly more than once #25

# v0.5.0 (2021-11-26)

Moving from `Newtonsoft.Json` to `Text.Json`.

## Issues Closed in this Release

### Maintenance

- Use Text.Json instead on Newtonsoft.Json #23

# v0.4.0 (2021-09-03)

Dependency updates only

# v0.3.0 (2021-07-02)

## Issues Closed in this Release

### New Features

- Decouple from Core #8

### Bug Fixes

- When version is null connector manager update should install latest version #15

# v0.2.1 (2021-06-03)

Bug fixes to support runtime-independent connector packages.

## Issues Closed in this Release

### Bug Fixes

- ConnectorPackage Extract should only flatten root of path #11
- Package ID should have correct capitalization #13

# v0.2.0 (2021-05-24)

Directory structure is now preserved when extracting connector packages.

### New Features

- Preserve subdirectory structure #10

# v0.1.0 (2021-05-15)

Initial release of the Connector Manager which provides functionality to:

- Add/Update/Remove connectors from an in-memory or file-based configuration
- Find and install connectors from a remote registry
- Load the connector assemblies, including any dependencies
- Bootstrap a configuration, downloading any missing connectors

## Issues Closed in this Release

### New Features

- Add IServiceCollection extension method to inject all the required configs #6
- Add a way to bootstrap a config #7
- Add ability to get assembly settings tuples #2
- Add unit tests for connector manager #4
- Add implementation of nuget logger #3





