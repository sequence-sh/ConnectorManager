using Reductech.EDR.ConnectorManagement.Base;

namespace Reductech.EDR.ConnectorManagement.Tests;

public class ConnectorDataTests
{
    [Fact]
    public void ToString_ReturnsIdAndVersion()
    {
        var id       = "Connector";
        var version  = "1.0.0";
        var settings = new ConnectorSettings { Id = id, Version = version };
        var cd       = new ConnectorData(settings, null);
        Assert.Equal($"{id} {version}", cd.ToString());
    }
}
