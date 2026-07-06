using Microsoft.Extensions.Configuration;
using NetDevs.ExceptionProcessor.Exceptions.Base;
using NetDevs.ExceptionProcessor.Settings;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;

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
    private static Logger Logger;
    private static NLogSettings _settings;

    /// <summary>
    /// Initializes the NLogExceptionLogger and loads settings from configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public static void Initialize(IConfiguration configuration)
    {
        _settings = configuration.GetSection("Logging:NLog").Get<NLogSettings>()
            ?? new NLogSettings();

        LoggingConfiguration config = new();
        FileTarget logfile = new("file")
        {
            FileName = _settings.LogFilePath,
            Layout = "${longdate} | ${level} | ${message}"
        };

        config.AddRule(LogLevel.Error, LogLevel.Fatal, logfile);
        LogManager.Configuration = config;
        Logger = LogManager.GetCurrentClassLogger();
    }

    /// <summary>
    /// Logs an exception using NLog.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    public void LogException(Exception exception)
    {
        if (Logger == null)
            throw new InvalidOperationException("NLogExceptionLogger must be initialized first");

        if (exception is BaseException baseEx)
            Logger.Error(baseEx, $"Base Exception - Code: {baseEx.Code}, HttpCode: {baseEx.HttpCode}");
        else
            Logger.Error(exception, "Unhandled Exception occurred");
    }
}