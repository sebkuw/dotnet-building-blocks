using System.Diagnostics;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace sebkuw.ExceptionProcessor.Middlewares;

/// <summary>
/// Middleware for global exception handling with dependency injection support.
/// 
/// Dependencies:
/// - IExceptionManager: Handles exception processing
/// - <see cref="ILogger{TCategoryName}"/>: Logs middleware operations
/// 
/// Parameters:
/// - next: Next middleware in pipeline
/// - exceptionManager: Injected exception manager
/// - logger: Injected logger
/// 
/// Returns: Task (InvokeAsync)
/// </summary>
public class GlobalExceptionMiddleware
{
    private static readonly Action<ILogger, Exception?> LogRequestException = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, "RequestException"),
        "Unhandled exception while processing the request.");

    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the GlobalExceptionMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <param name="logger">The injected logger.</param>
    public GlobalExceptionMiddleware(
        RequestDelegate next,
        IServiceProvider serviceProvider,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>Processes the request and converts unhandled exceptions to JSON responses.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that represents middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Pobierz scoped serwis z IServiceProvider na czas obsługi błędu
        var exceptionManager = _serviceProvider.GetRequiredService<IExceptionManager>();

        string correlationId = context.Items.TryGetValue("CorrelationId", out object? item)
            && item is string value
            && !string.IsNullOrWhiteSpace(value)
                ? value
                : Activity.Current?.Id ?? context.TraceIdentifier;

        var response = exceptionManager.HandleException(exception, correlationId);

        context.Response.StatusCode = response.HttpCode;
        context.Response.ContentType = "application/json";
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        LogRequestException(_logger, exception);

        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}
