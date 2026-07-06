using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NetDevs.ExceptionProcessor.Exceptions.Custom;
using NetDevs.ExceptionProcessor.Loggers;
using NetDevs.ExceptionProcessor.Middlewares;
using NetDevs.ExceptionProcessor.Models;
using Xunit;

namespace NetDevs.ExceptionProcessor.Tests;

public sealed class ExceptionProcessorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void ExceptionManager_maps_base_exception_to_configured_response()
    {
        var logger = new TestExceptionLogger();
        var manager = new ExceptionManager(logger);
        var exception = new ObjectNotFoundException("User", 123);

        ExceptionResponse response = manager.HandleException(exception, "trace-1");

        Assert.Same(exception, logger.LoggedException);
        Assert.Equal("ObjectNotFound", response.Code);
        Assert.Equal(404, response.HttpCode);
        Assert.Equal("Object not found", response.MainText);
        Assert.Equal("The requested User with ID 123 was not found.", response.Description);
        Assert.Equal("trace-1", response.TraceId);
    }

    [Fact]
    public void ExceptionManager_maps_unknown_exception_to_unhandled_response()
    {
        var logger = new TestExceptionLogger();
        var manager = new ExceptionManager(logger);
        var exception = new InvalidOperationException("Something broke");

        ExceptionResponse response = manager.HandleException(exception, "trace-2");

        Assert.Same(exception, logger.LoggedException);
        Assert.Equal("UnhandledException", response.Code);
        Assert.Equal(500, response.HttpCode);
        Assert.Equal("An unexpected error occurred", response.MainText);
        Assert.Equal("Something broke", response.Description);
        Assert.Equal("trace-2", response.TraceId);
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_writes_structured_json_response()
    {
        var services = new ServiceCollection();
        var logger = new TestExceptionLogger();

        services.AddSingleton<IExceptionLogger>(logger);
        services.AddSingleton<IExceptionManager, ExceptionManager>();

        await using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        var context = new DefaultHttpContext
        {
            RequestServices = provider,
            TraceIdentifier = "trace-3"
        };

        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var middleware = new GlobalExceptionMiddleware(
            _ => throw new ValidationException("Email is required"),
            provider,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        responseBody.Position = 0;
        ExceptionResponse response = await JsonSerializer.DeserializeAsync<ExceptionResponse>(
            responseBody,
            JsonOptions) ?? throw new InvalidOperationException("Response body was empty.");

        Assert.IsType<ValidationException>(logger.LoggedException);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal("trace-3", context.Response.Headers["X-Correlation-ID"]);
        Assert.Equal("ValidationError", response.Code);
        Assert.Equal(StatusCodes.Status400BadRequest, response.HttpCode);
        Assert.Equal("Invalid input", response.MainText);
        Assert.Equal("Validation failed: Email is required", response.Description);
        Assert.Equal("trace-3", response.TraceId);
    }

    [Fact]
    public async Task TraceIdMiddleware_uses_request_correlation_id_when_present()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "client-trace";

        var middleware = new TraceIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("client-trace", context.Items["CorrelationId"]);
        Assert.Equal("client-trace", context.Response.Headers["X-Correlation-ID"]);
    }

    [Fact]
    public async Task TraceIdMiddleware_generates_correlation_id_when_header_is_missing()
    {
        var context = new DefaultHttpContext();

        var middleware = new TraceIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        string correlationId = Assert.IsType<string>(context.Items["CorrelationId"]);

        Assert.True(Guid.TryParse(correlationId, out _));
        Assert.Equal(correlationId, context.Response.Headers["X-Correlation-ID"]);
    }

    private sealed class TestExceptionLogger : IExceptionLogger
    {
        public Exception? LoggedException { get; private set; }

        public void LogException(Exception exception)
        {
            LoggedException = exception;
        }
    }
}
