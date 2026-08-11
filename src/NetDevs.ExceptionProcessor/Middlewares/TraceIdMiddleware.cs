using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NetDevs.ExceptionProcessor.Middlewares;

/// <summary>
/// Establishes and propagates an HTTP correlation identifier.
/// </summary>
public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Initializes the middleware.</summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>Preserves a client correlation ID or creates a new one.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that represents middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Sprawdź czy client przesłał Correlation ID
        string correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues id)
            && !StringValues.IsNullOrEmpty(id)
            ? id.ToString()
            : Guid.NewGuid().ToString();

        // Zapisz w context
        context.Items["CorrelationId"] = correlationId;

        // Propaguj w response
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        await _next(context);
    }
}
