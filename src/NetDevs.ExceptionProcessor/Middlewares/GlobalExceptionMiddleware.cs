using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetDevs.ExceptionProcessor.Middlewares;

/// <summary>
/// Middleware for global exception handling with dependency injection support.
/// 
/// Dependencies:
/// - IExceptionManager: Handles exception processing
/// - ILogger<GlobalExceptionMiddleware>: Logs middleware operations
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
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the GlobalExceptionMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="exceptionManager">The injected exception manager.</param>
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

        string correlationId = Activity.Current?.Id ?? context.TraceIdentifier;

        var response = exceptionManager.HandleException(exception, correlationId);

        context.Response.StatusCode = response.HttpCode;
        context.Response.ContentType = "application/json";
        context.Response.Headers.Add("X-Correlation-ID", correlationId);

        _logger.LogError(exception, exception.Message);

        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}