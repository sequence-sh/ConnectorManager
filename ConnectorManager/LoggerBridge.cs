using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Nu = NuGet.Common;

namespace Reductech.Sequence.ConnectorManagement;

/// <inheritdoc />
public class LoggerBridge<T> : Nu.ILogger
{
    private readonly ILogger<T> _logger;

    /// <summary>
    /// Create a new NuGet LoggerBridge using the given logger.
    /// </summary>
    public LoggerBridge(ILogger<T> logger) => _logger = logger;

    /// <inheritdoc />
    public void LogDebug(string data) => _logger.LogDebug(data);

    /// <inheritdoc />
    public void LogVerbose(string data) => _logger.LogTrace(data);

    /// <inheritdoc />
    public void LogInformation(string data) => _logger.LogInformation(data);

    /// <inheritdoc />
    public void LogMinimal(string data) => _logger.LogInformation(data);

    /// <inheritdoc />
    public void LogWarning(string data) => _logger.LogWarning(data);

    /// <inheritdoc />
    public void LogError(string data) => _logger.LogError(data);

    /// <inheritdoc />
    public void LogInformationSummary(string data) => _logger.LogDebug(data);

    /// <inheritdoc />
    public void Log(Nu.LogLevel level, string data) => _logger.Log(ToLogLevel(level), data);

    /// <inheritdoc />
    public Task LogAsync(Nu.LogLevel level, string data) => Task.Run(() => Log(level, data));

    /// <inheritdoc />
    public void Log(Nu.ILogMessage message) => _logger.Log(
        ToLogLevel(message.Level),
        new EventId((int)message.Code, message.Code.ToString()),
        message.Message
    );

    /// <inheritdoc />
    public Task LogAsync(Nu.ILogMessage message) => Task.Run(() => Log(message));

    /// <summary>
    /// Convert NuGet LogLevel to Microsoft.Extensions.Logging LogLevel
    /// </summary>
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
