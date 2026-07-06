using System;

namespace NetDevs.ExceptionProcessor.Loggers;

/// <summary>
/// Interface for exception logging implementations.
/// 
/// Methods:
/// - LogException(Exception): Logs exception to configured destination
/// 
/// Parameters:
/// - exception: The exception to log
/// 
/// Returns: void
/// </summary>
public interface IExceptionLogger
{
    /// <summary>
    /// Logs an exception using the configured logging mechanism.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    void LogException(Exception exception);
}