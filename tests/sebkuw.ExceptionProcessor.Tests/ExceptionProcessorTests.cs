using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NLog;

using sebkuw.ExceptionProcessor.Exceptions.Base;
using sebkuw.ExceptionProcessor.Exceptions.Custom;
using sebkuw.ExceptionProcessor.Loggers;
using sebkuw.ExceptionProcessor.Middlewares;
using sebkuw.ExceptionProcessor.Models;

using Xunit;

using ProcessorTimeoutException = sebkuw.ExceptionProcessor.Exceptions.Custom.TimeoutException;
using ProcessorUnauthorizedAccessException = sebkuw.ExceptionProcessor.Exceptions.Custom.UnauthorizedAccessException;

namespace sebkuw.ExceptionProcessor.Tests;

public sealed class ExceptionProcessorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void ExceptionManager_rejects_missing_logger()
    {
        Assert.Throws<ArgumentNullException>(() => new ExceptionManager(null!));
    }

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
        Assert.Equal("An unexpected error occurred. Please contact support with the trace ID.", response.Description);
        Assert.Equal("trace-2", response.TraceId);
    }

    [Fact]
    public void ExceptionManager_rejects_missing_exception()
    {
        var logger = new TestExceptionLogger();
        var manager = new ExceptionManager(logger);

        Assert.Throws<ArgumentNullException>(() => manager.HandleException(null!));

        Assert.Null(logger.LoggedException);
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_resolves_manager_from_request_scope_and_hides_internal_details()
    {
        var services = new ServiceCollection();
        services.AddScoped<IExceptionLogger, TestExceptionLogger>();
        services.AddScoped<IExceptionManager, ExceptionManager>();
        await using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();
        var logger = (TestExceptionLogger)scope.ServiceProvider.GetRequiredService<IExceptionLogger>();
        var exception = new InvalidOperationException("Password=private-value; server=C:\\internal\\database");
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Items["CorrelationId"] = "safe-trace";
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var middlewareLogger = new RecordingMiddlewareLogger();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception, provider, middlewareLogger);

        await middleware.InvokeAsync(context);

        body.Position = 0;
        using var document = await JsonDocument.ParseAsync(body);
        var response = document.RootElement;
        Assert.Same(exception, logger.LoggedException);
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal("safe-trace", context.Response.Headers["X-Correlation-ID"]);
        Assert.Equal("safe-trace", response.GetProperty("traceId").GetString());
        Assert.Equal("UnhandledException", response.GetProperty("code").GetString());
        Assert.Equal(500, response.GetProperty("httpCode").GetInt32());
        Assert.Equal("An unexpected error occurred", response.GetProperty("mainText").GetString());
        Assert.Equal("An unexpected error occurred. Please contact support with the trace ID.", response.GetProperty("description").GetString());
        Assert.Equal(DateTimeKind.Utc, response.GetProperty("timestamp").GetDateTime().Kind);
        Assert.Equal(6, response.EnumerateObject().Count());
        Assert.DoesNotContain("private-value", response.GetRawText(), StringComparison.Ordinal);
        Assert.DoesNotContain("internal", response.GetRawText(), StringComparison.Ordinal);
        var logScope = Assert.IsType<Dictionary<string, object>>(middlewareLogger.ScopeState);
        Assert.Equal("safe-trace", logScope["CorrelationId"]);
        Assert.Same(exception, middlewareLogger.LoggedException);
    }

    [Fact]
    public void GlobalExceptionMiddleware_rejects_missing_service_provider()
    {
        Assert.Throws<ArgumentNullException>(() => new GlobalExceptionMiddleware(
            _ => Task.CompletedTask, null!, NullLogger<GlobalExceptionMiddleware>.Instance));
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_handles_cancellation_unrelated_to_request_abort()
    {
        var services = new ServiceCollection();
        var logger = new TestExceptionLogger();
        services.AddSingleton<IExceptionLogger>(logger);
        services.AddScoped<IExceptionManager, ExceptionManager>();
        await using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var exception = new OperationCanceledException("Application-owned cancellation");
        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception, provider, NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        body.Position = 0;
        using var document = await JsonDocument.ParseAsync(body);
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("UnhandledException", document.RootElement.GetProperty("code").GetString());
        Assert.Same(exception, logger.LoggedException);
    }

    private sealed class RecordingMiddlewareLogger : ILogger<GlobalExceptionMiddleware>
    {
        public object? ScopeState { get; private set; }

        public Exception? LoggedException { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            ScopeState = state;
            return null;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LoggedException = exception;
        }
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_preserves_exception_when_response_has_started()
    {
        await using var provider = new ServiceCollection().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());
        var exception = new InvalidOperationException("Streaming failed");
        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception, provider, NullLogger<GlobalExceptionMiddleware>.Instance);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        Assert.Same(exception, actual);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_propagates_request_cancellation_without_error_response()
    {
        await using var provider = new ServiceCollection().BuildServiceProvider();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = new DefaultHttpContext { RequestServices = provider, RequestAborted = cancellation.Token };
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var exception = new OperationCanceledException(cancellation.Token);
        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception, provider, NullLogger<GlobalExceptionMiddleware>.Instance);

        var actual = await Assert.ThrowsAsync<OperationCanceledException>(() => middleware.InvokeAsync(context));

        Assert.Same(exception, actual);
        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal(0, body.Length);
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_replaces_buffered_response_with_error_json()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IExceptionLogger, TestExceptionLogger>();
        services.AddScoped<IExceptionManager, ExceptionManager>();
        await using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var middleware = new GlobalExceptionMiddleware(async httpContext =>
        {
            await httpContext.Response.WriteAsync("partial content that must be discarded");
            httpContext.Response.Headers["Content-Length"] = "999";
            throw new ValidationException("Invalid input");
        }, provider, NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        body.Position = 0;
        using var document = await JsonDocument.ParseAsync(body);
        Assert.Equal("ValidationError", document.RootElement.GetProperty("code").GetString());
        Assert.False(context.Response.Headers.ContainsKey("Content-Length"));
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TraceIdMiddleware_generates_correlation_id_for_blank_header(string header)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = header;
        var middleware = new TraceIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var correlationId = Assert.IsType<string>(context.Items["CorrelationId"]);
        Assert.True(Guid.TryParse(correlationId, out _));
        Assert.Equal(correlationId, context.Response.Headers["X-Correlation-ID"]);
    }

    private sealed class StartedResponseFeature : HttpResponseFeature
    {
        public override bool HasStarted => true;
    }

    [Fact]
    public void Built_in_exceptions_expose_stable_status_and_error_codes()
    {
        BaseException[] exceptions =
        [
            new DatabaseException("unavailable"),
            new FileNotExistException("report.pdf"),
            new ObjectNotFoundException("User", 123),
            new OperationFailedException("conflict"),
            new ProcessorTimeoutException(),
            new ProcessorUnauthorizedAccessException(),
            new ValidationException("Email is required")
        ];

        Assert.Collection(
            exceptions,
            exception => AssertContract(exception, "DatabaseError", 500),
            exception => AssertContract(exception, "FileNotExist", 404),
            exception => AssertContract(exception, "ObjectNotFound", 404),
            exception => AssertContract(exception, "OperationFailed", 500),
            exception => AssertContract(exception, "TimeoutError", 408),
            exception => AssertContract(exception, "UnauthorizedAccess", 403),
            exception => AssertContract(exception, "ValidationError", 400));
    }

    [Fact]
    public void ExceptionLogger_initializes_and_logs_base_and_unhandled_exceptions()
    {
        string logPath = Path.Combine(Path.GetTempPath(), $"sebkuw-exceptions-{Guid.NewGuid():N}.log");
        try
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Logging:NLog:LogFilePath"] = logPath
                })
                .Build();
            ExceptionLogger.Initialize(configuration);
            var logger = new ExceptionLogger();

            logger.LogException(new ValidationException("Invalid value"));
            logger.LogException(new InvalidOperationException("Unexpected failure"));
            LogManager.Flush();
            LogManager.Shutdown();

            string log = File.ReadAllText(logPath);
            Assert.Contains("Base Exception", log, StringComparison.Ordinal);
            Assert.Contains("Unhandled Exception", log, StringComparison.Ordinal);
        }
        finally
        {
            LogManager.Shutdown();
            if (File.Exists(logPath))
                File.Delete(logPath);
        }
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

    [Fact]
    public async Task Middleware_pipeline_preserves_client_correlation_id_in_error_contract()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IExceptionLogger, TestExceptionLogger>();
        services.AddSingleton<IExceptionManager, ExceptionManager>();

        await using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        var context = new DefaultHttpContext
        {
            RequestServices = provider,
            TraceIdentifier = "server-trace"
        };
        context.Request.Headers["X-Correlation-ID"] = "angular-trace";

        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var exceptionMiddleware = new GlobalExceptionMiddleware(
            _ => throw new ValidationException("Invalid filter"),
            provider,
            NullLogger<GlobalExceptionMiddleware>.Instance);
        var traceMiddleware = new TraceIdMiddleware(exceptionMiddleware.InvokeAsync);

        await traceMiddleware.InvokeAsync(context);

        responseBody.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(responseBody);

        Assert.Equal("angular-trace", context.Response.Headers["X-Correlation-ID"]);
        Assert.Equal("angular-trace", document.RootElement.GetProperty("traceId").GetString());
    }

    private sealed class TestExceptionLogger : IExceptionLogger
    {
        public Exception? LoggedException { get; private set; }

        public void LogException(Exception exception)
        {
            LoggedException = exception;
        }
    }

    private static void AssertContract(BaseException exception, string code, int httpCode)
    {
        Assert.Equal(code, exception.Code);
        Assert.Equal(httpCode, exception.HttpCode);
        Assert.False(string.IsNullOrWhiteSpace(exception.MainText));
        Assert.False(string.IsNullOrWhiteSpace(exception.Description));
    }
}
