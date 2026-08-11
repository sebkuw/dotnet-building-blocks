# NetDevs.ExceptionProcessor

Reusable ASP.NET Core exception mapping, structured JSON errors, correlation IDs, and replaceable exception logging for .NET 10 APIs.

## Installation

```powershell
dotnet add package NetDevs.ExceptionProcessor
```

## Register services

```csharp
builder.Services.AddScoped<IExceptionManager, ExceptionManager>();
builder.Services.AddSingleton<IExceptionLogger, ExceptionLogger>();
ExceptionLogger.Initialize(builder.Configuration);
```

## Configure middleware

Order matters: correlation must be established before exception processing.

```csharp
app.UseMiddleware<TraceIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
```

The middleware accepts a non-empty `X-Correlation-ID` or generates one, then preserves it in `HttpContext.Items["CorrelationId"]`, the response header, and the `traceId` error field.

## Error contract

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

Derive application errors from `BaseException` to provide a stable machine code, status, safe title, and safe description. Unknown failures map to HTTP 500. Never put secrets, stack traces, or infrastructure details in client-facing descriptions.

See the [Angular compatibility contract](../../docs/angular-shared-components-contract.md).

## Development

- [Library instructions](AGENTS.md)
- [Development skill](../../.agents/skills/develop-netdevs-exception-processor/SKILL.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/NetDevs.ExceptionProcessor.Tests`

```powershell
dotnet test tests/NetDevs.ExceptionProcessor.Tests/NetDevs.ExceptionProcessor.Tests.csproj
```
