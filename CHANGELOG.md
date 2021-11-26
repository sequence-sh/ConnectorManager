# v0.5.0 (2021-11-26)

## Issues Closed in this Release

### Maintenance

- Use Text.Json instead on Newtonsoft.Json #23
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

