using Microsoft.Extensions.Configuration;

using NetDevs.ExceptionProcessor.Exceptions.Base;
using NetDevs.ExceptionProcessor.Settings;

using NLog;
using NLog.Config;
using NLog.Targets;

namespace NetDevs.ExceptionProcessor.Loggers;

/// <summary>
/// Handles exception logging using NLog.
/// Implements IExceptionLogger for dependency injection support.
/// 
/// Methods:
/// - Initialize(IConfiguration): Configures NLog from appsettings
/// - LogException(Exception): Logs exception to file
/// 
/// Parameters:
/// - configuration: Configuration source for NLog settings
/// - exception: Exception to log
/// 
/// Returns: void (static Initialize), void (LogException)
/// </summary>
public class ExceptionLogger : IExceptionLogger
{
    private static Logger? _logger;

    /// <summary>
    /// Initializes the NLogExceptionLogger and loads settings from configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public static void Initialize(IConfiguration configuration)
    {
        NLogSettings settings = configuration.GetSection("Logging:NLog").Get<NLogSettings>()
            ?? new NLogSettings();

        LoggingConfiguration config = new();
        FileTarget logfile = new("file")
        {
            FileName = settings.LogFilePath,
            Layout = "${longdate} | ${level} | ${message}"
        };

        config.AddRule(LogLevel.Error, LogLevel.Fatal, logfile);
        LogManager.Configuration = config;
        _logger = LogManager.GetCurrentClassLogger();
    }

    /// <summary>
    /// Logs an exception using NLog.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    public void LogException(Exception exception)
    {
        if (_logger is null)
            throw new InvalidOperationException("NLogExceptionLogger must be initialized first");

        if (exception is BaseException baseEx)
            _logger.Error(baseEx, "Base Exception - Code: {Code}, HttpCode: {HttpCode}", baseEx.Code, baseEx.HttpCode);
        else
            _logger.Error(exception, "Unhandled Exception occurred");
    }
}
