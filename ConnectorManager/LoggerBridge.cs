using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nu = NuGet.Common;

namespace Reductech.EDR.ConnectorManagement
{

public class LoggerBridge<T> : Nu.ILogger
{
    private readonly ILogger<T> _logger;

    public LoggerBridge(ILogger<T> logger) => _logger = logger;

    public void LogDebug(string data) => _logger.LogDebug(data);

    public void LogVerbose(string data) => _logger.LogTrace(data);

    public void LogInformation(string data) => _logger.LogInformation(data);

    public void LogMinimal(string data) => _logger.LogInformation(data);

    public void LogWarning(string data) => _logger.LogWarning(data);

    public void LogError(string data) => _logger.LogError(data);

    public void LogInformationSummary(string data) => _logger.LogDebug(data);

    public void Log(Nu.LogLevel level, string data) => _logger.Log(ToLogLevel(level), data);

    public Task LogAsync(Nu.LogLevel level, string data) => Task.Run(() => Log(level, data));

    public void Log(Nu.ILogMessage message) => _logger.Log(
        ToLogLevel(message.Level),
        new EventId((int)message.Code, message.Code.ToString()),
        message.Message
    );

    public Task LogAsync(Nu.ILogMessage message) => Task.Run(() => Log(message));

    public static LogLevel ToLogLevel(Nu.LogLevel logLevel) => logLevel switch
    {
        Nu.LogLevel.Verbose     => LogLevel.Trace,
        Nu.LogLevel.Debug       => LogLevel.Debug,
        Nu.LogLevel.Information => LogLevel.Information,
        Nu.LogLevel.Minimal     => LogLevel.Information,
        Nu.LogLevel.Warning     => LogLevel.Warning,
        Nu.LogLevel.Error       => LogLevel.Error,
        _                       => throw new ArgumentOutOfRangeException(nameof(logLevel))
    };
}

}
