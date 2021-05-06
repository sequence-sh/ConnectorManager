namespace Reductech.EDR.ConnectorManagement.Tests
{

internal static class Helpers
{
    internal static readonly ConnectorRegistrySettings IntegrationRegistrySettings =
        new(
            "https://gitlab.com/api/v4/projects/26301248/packages/nuget/index.json",
            "integrationtests",
            "E8YL7f4kTM4XJEn1ixnL"
        );
}

}
