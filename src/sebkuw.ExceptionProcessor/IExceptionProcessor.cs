using sebkuw.ExceptionProcessor.Models;

namespace sebkuw.ExceptionProcessor;

/// <summary>
/// Interface for exception handling and processing.
/// 
/// Parameters:
/// - exception: The exception to handle
/// - correlationId: Optional trace ID for distributed tracing
/// 
/// Returns: ExceptionResponse with standardized error information
/// </summary>
public interface IExceptionManager
{
    /// <summary>
    /// Processes an exception and converts it into a standardized ExceptionResponse.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="correlationId">Optional correlation ID for tracing.</param>
    /// <returns>Standardized ExceptionResponse object.</returns>
    ExceptionResponse HandleException(Exception exception, string? correlationId = null);
}
