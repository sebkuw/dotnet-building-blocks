using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace NetDevs.ExceptionProcessor.Middlewares;

public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Sprawdź czy client przesłał Correlation ID
        string correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues id)
            ? id.ToString()
            : Guid.NewGuid().ToString();

        // Zapisz w context
        context.Items["CorrelationId"] = correlationId;

        // Propaguj w response
        context.Response.Headers.Add("X-Correlation-ID", correlationId);

        await _next(context);
    }
}
