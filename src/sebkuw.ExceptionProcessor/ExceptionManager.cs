using sebkuw.ExceptionProcessor.Exceptions.Base;
using sebkuw.ExceptionProcessor.Loggers;
using sebkuw.ExceptionProcessor.Models;

namespace sebkuw.ExceptionProcessor;

/// <summary>
/// Manages exception handling and logging with dependency injection support.
/// 
/// Methods:
/// - HandleException(Exception, string?): Processes exceptions into standardized responses
/// 
/// Parameters:
/// - exception: The exception to process
/// - correlationId: Optional trace ID for request tracking
/// 
/// Returns: ExceptionResponse with error details
/// </summary>
public class ExceptionManager : IExceptionManager
{
    private readonly IExceptionLogger _logger;

    /// <summary>
    /// Initializes a new instance of the ExceptionManager class.
    /// </summary>
    /// <param name="logger">The exception logger implementation.</param>
    public ExceptionManager(IExceptionLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes an exception and converts it into a standardized ExceptionResponse.
    /// </summary>
    /// <param name="exception">The exception to process.</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing.</param>
    /// <returns>An ExceptionResponse containing standardized error details.</returns>
    public ExceptionResponse HandleException(Exception exception, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Log the exception using injected logger
        _logger.LogException(exception);

        // Convert to standardized response
        if (exception is BaseException baseEx)
            return new ExceptionResponse(
                baseEx.Code,
                baseEx.HttpCode,
                baseEx.MainText,
                baseEx.Description,
                correlationId);

        return new ExceptionResponse(
            "UnhandledException",
            500,
            "An unexpected error occurred",
            "An unexpected error occurred. Please contact support with the trace ID.",
            correlationId);
    }
}
