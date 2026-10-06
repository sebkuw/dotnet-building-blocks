# sebkuw.ExceptionProcessor

ASP.NET Core exception mapping, structured JSON errors, correlation IDs, and replaceable exception logging for .NET 10 APIs.

## Installation

```powershell
dotnet add package sebkuw.ExceptionProcessor
```

The package references the ASP.NET Core shared framework and includes an NLog adapter. Applications may replace the logging implementation through `IExceptionLogger`.

## Register services

In an ASP.NET Core application's `Program.cs`:

```csharp
using sebkuw.ExceptionProcessor;
using sebkuw.ExceptionProcessor.Loggers;
using sebkuw.ExceptionProcessor.Middlewares;

var builder = WebApplication.CreateBuilder(args);

ExceptionLogger.Initialize(builder.Configuration);
builder.Services.AddSingleton<IExceptionLogger, ExceptionLogger>();
builder.Services.AddScoped<IExceptionManager, ExceptionManager>();

var app = builder.Build();

app.UseMiddleware<TraceIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapGet("/failure", () =>
    Task.FromException<string>(new InvalidOperationException("Internal diagnostic detail")));

app.Run();
```

Register correlation middleware before exception middleware, and place both before endpoints or other middleware whose failures should be handled. `GlobalExceptionMiddleware` resolves `IExceptionManager` from the current request's service scope.

For the bundled logger, configure a writable log path in `appsettings.json`:

```json
{
  "Logging": {
    "NLog": {
      "LogFilePath": "logs/exceptions.log"
    }
  }
}
```

`ExceptionLogger.Initialize` must run before logging. It changes process-wide NLog configuration; applications already managing NLog should supply their own `IExceptionLogger` adapter instead. The default path is `logs/app.log`.

## Error contract

The middleware serializes these six explicitly named fields, independently of the application's JSON naming policy:

```json
{
  "code": "ObjectNotFound",
  "httpCode": 404,
  "mainText": "Object not found",
  "description": "The requested User with ID 123 was not found.",
  "timestamp": "2026-08-11T12:00:00Z",
  "traceId": "request-correlation-id"
}
```

`ExceptionResponse.Timestamp` is initialized from `DateTime.UtcNow`. `TraceIdMiddleware` accepts a non-whitespace `X-Correlation-ID` or generates a GUID, then preserves it in `HttpContext.Items["CorrelationId"]`, the response header, and the error body. Without that middleware, the error handler falls back to `Activity.Current.Id` or `HttpContext.TraceIdentifier`.

The middleware adds a `CorrelationId` scope to its Microsoft `ILogger` entry; configure a provider that records scopes to persist it. The bundled `IExceptionLogger` interface accepts only an exception, and its default NLog adapter does not automatically include the correlation ID or exception stack trace in its file layout. Supply an application logging adapter when those diagnostics are needed.

## Built-in mappings

| Exception | Code | HTTP status |
| --- | --- | ---: |
| `ValidationException` | `ValidationError` | 400 |
| `UnauthorizedAccessException` | `UnauthorizedAccess` | 403 |
| `ObjectNotFoundException` | `ObjectNotFound` | 404 |
| `FileNotExistException` | `FileNotExist` | 404 |
| `TimeoutException` | `TimeoutError` | 408 |
| `DatabaseException` | `DatabaseError` | 500 |
| `OperationFailedException` | `OperationFailed` | 500 |
| Any exception outside `BaseException` | `UnhandledException` | 500 |

The custom exceptions are in `sebkuw.ExceptionProcessor.Exceptions.Custom`. `TimeoutException` and `UnauthorizedAccessException` are distinct from the similarly named `System` exceptions; the latter use the unknown-exception mapping.

Derive application errors from `BaseException` to provide a stable machine code, status, title, and description:

```csharp
using sebkuw.ExceptionProcessor.Exceptions.Base;

public sealed class DuplicateOrderException : BaseException
{
    public DuplicateOrderException()
        : base("DuplicateOrder", 409, "Duplicate order", "An order with this reference already exists.")
    {
    }
}
```

## Security and failure behavior

- Unknown failures return HTTP 500 with `mainText` set to `An unexpected error occurred` and a fixed description: `An unexpected error occurred. Please contact support with the trace ID.` The original exception is passed to the logger; its message is not returned to the client.
- Descriptions supplied by `BaseException`, including built-in custom exception arguments, are sent to clients verbatim. Use safe display text rather than database errors, server paths, secrets, or personal data.
- Once an HTTP response has started, exceptions propagate to the host; the middleware cannot replace a streaming response with JSON. Buffered, unstarted responses are cleared before an error is written.
- Cancellation caused by an aborted request propagates without conversion into an HTTP 500 response. Output writes observe `HttpContext.RequestAborted`.
- Exceptions thrown by an application exception manager or logger propagate. Supply a reliable adapter and configure logging before requests arrive.
- Correlation IDs provide diagnostics, not authentication or authorization. The package does not install access-control policies.

Consumers upgrading from the earlier unknown-error behavior should use `code` and `traceId` for diagnostics instead of expecting the original exception message in `description`. The JSON field names, error code, and HTTP status are preserved.

See the [Angular compatibility contract](https://github.com/sebkuw/dotnet-building-blocks/blob/main/docs/angular-shared-components-contract.md).

## Development

Run from the repository root:

```powershell
dotnet test tests/sebkuw.ExceptionProcessor.Tests/sebkuw.ExceptionProcessor.Tests.csproj
```

Tests cover mappings, scoped DI, exact JSON fields, correlation, information disclosure, request cancellation, and failures after response start. See the [changelog](https://github.com/sebkuw/dotnet-building-blocks/blob/main/src/sebkuw.ExceptionProcessor/CHANGELOG.md) and [repository verification guide](https://github.com/sebkuw/dotnet-building-blocks#verification).
