using System.Linq;
using System.Threading.Tasks;
using MELT;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using Xunit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Reductech.EDR.ConnectorManagement.Tests;

public class LoggerBridgeTests
{
    private readonly ITestLoggerFactory _loggerFactory;

    public LoggerBridgeTests()
    {
        _loggerFactory = TestLoggerFactory.Create();
    }

    [Theory]
    [InlineData("LogDebug",              LogLevel.Debug)]
    [InlineData("LogVerbose",            LogLevel.Trace)]
    [InlineData("LogInformation",        LogLevel.Information)]
    [InlineData("LogMinimal",            LogLevel.Information)]
    [InlineData("LogWarning",            LogLevel.Warning)]
    [InlineData("LogError",              LogLevel.Error)]
    [InlineData("LogInformationSummary", LogLevel.Debug)]
    public void LogLevelMethods_LogTheRightLogLevel(string method, LogLevel logLevel)
    {
        const string message = "test";

        var lb = new LoggerBridge<LoggerBridgeTests>(
            _loggerFactory.CreateLogger<LoggerBridgeTests>()
        );

        lb.GetType().GetMethod(method)!.Invoke(lb, new object?[] { message });

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == logLevel && l.Message!.Equals(message)
        );
    }

    [Theory]
    [InlineData(NuGet.Common.LogLevel.Debug,       LogLevel.Debug)]
    [InlineData(NuGet.Common.LogLevel.Verbose,     LogLevel.Trace)]
    [InlineData(NuGet.Common.LogLevel.Information, LogLevel.Information)]
    [InlineData(NuGet.Common.LogLevel.Minimal,     LogLevel.Information)]
    [InlineData(NuGet.Common.LogLevel.Warning,     LogLevel.Warning)]
    [InlineData(NuGet.Common.LogLevel.Error,       LogLevel.Error)]
    public void Log_LogsMessageWithCorrectLogLevel(
        NuGet.Common.LogLevel inputLogLevel,
        LogLevel outputLogLevel)
    {
        const string message = "test";

        var lb = new LoggerBridge<LoggerBridgeTests>(
            _loggerFactory.CreateLogger<LoggerBridgeTests>()
        );

        lb.Log(inputLogLevel, message);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == outputLogLevel && l.Message!.Equals(message)
        );
    }

    [Fact]
    public async Task LogAsync_LogsMessageWithCorrectLogLevel()
    {
        const string message = "test";

        var lb = new LoggerBridge<LoggerBridgeTests>(
            _loggerFactory.CreateLogger<LoggerBridgeTests>()
        );

        await lb.LogAsync(NuGet.Common.LogLevel.Minimal, message);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == LogLevel.Information && l.Message!.Equals(message)
        );
    }

    [Fact]
    public void Log_WhenArgIsILogMessage_LogsMessage()
    {
        const string message = "test";

        var lb = new LoggerBridge<LoggerBridgeTests>(
            _loggerFactory.CreateLogger<LoggerBridgeTests>()
        );

        var imessage = new LogMessage(NuGet.Common.LogLevel.Minimal, message);

        lb.Log(imessage);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == LogLevel.Information && l.Message!.Equals(message)
        );
    }

    [Fact]
    public async Task LogAsync_WhenArgIsILogMessage_LogsMessage()
    {
        const string message = "test";

        var lb = new LoggerBridge<LoggerBridgeTests>(
            _loggerFactory.CreateLogger<LoggerBridgeTests>()
        );

        var imessage = new LogMessage(NuGet.Common.LogLevel.Minimal, message);

        await lb.LogAsync(imessage);

        Assert.Contains(
            _loggerFactory.GetTestLoggerSink().LogEntries.ToArray(),
            l => l.LogLevel == LogLevel.Information && l.Message!.Equals(message)
        );
    }
}
