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
