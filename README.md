# sebkuw Building Blocks

Reusable .NET building blocks for Clean Architecture applications.

This repository contains small, focused packages that keep application and domain code independent from infrastructure concerns. The current packages cover domain entities, auditing contracts, CQRS contracts, CQRS handler registration for Microsoft dependency injection, EF Core auditing, API exception processing and dynamic query processing.

## Table of Contents

- [Packages](#packages)
- [Repository Structure](#repository-structure)
- [sebkuw.Domain.Abstractions](#sebkuwdomainabstractions)
- [sebkuw.Cqrs.Abstractions](#sebkuwcqrsabstractions)
- [sebkuw.Cqrs](#sebkuwcqrs)
- [sebkuw.EntityFrameworkCore.Auditing](#sebkuwentityframeworkcoreauditing)
- [sebkuw.ExceptionProcessor](#sebkuwexceptionprocessor)
- [sebkuw.QueryableProcessor](#sebkuwqueryableprocessor)
- [Build](#build)
- [Tests](#tests)
- [Project Standards](#project-standards)
- [Changelog](#changelog)
- [License](#license)

## Packages

| Package | Purpose |
| --- | --- |
| [`sebkuw.Domain.Abstractions`](src/sebkuw.Domain.Abstractions/README.md) | Base entity, auditing, deactivation, and persistence lifecycle contracts for domain models. |
| [`sebkuw.Cqrs.Abstractions`](src/sebkuw.Cqrs.Abstractions/README.md) | Dependency-free CQRS contracts for commands, queries and handlers. |
| [`sebkuw.Cqrs`](src/sebkuw.Cqrs/README.md) | Automatic CQRS handler registration for `Microsoft.Extensions.DependencyInjection` using Scrutor. |
| [`sebkuw.EntityFrameworkCore.Auditing`](src/sebkuw.EntityFrameworkCore.Auditing/README.md) | EF Core interceptors for automatic audit metadata and entity lifecycle enforcement. |
| [`sebkuw.ExceptionProcessor`](src/sebkuw.ExceptionProcessor/README.md) | ASP.NET Core exception middleware, correlation IDs and structured JSON error responses. |
| [`sebkuw.QueryableProcessor`](src/sebkuw.QueryableProcessor/README.md) | Dynamic `IQueryable` filtering, sorting, pagination and Angular-compatible response metadata. |

## Migration to the `sebkuw` package prefix

Version 2.0.0 changes the package identity and namespaces of every library. Replace each existing package reference and `using` directive with the matching `sebkuw.*` name. The solution, production projects, test projects, assembly names, and the auditing extension `AddSebkuwAuditingInterceptors()` use the same prefix.

## Repository Structure

```text
src/
  sebkuw.Domain.Abstractions/
  sebkuw.Cqrs.Abstractions/
  sebkuw.Cqrs/
  sebkuw.EntityFrameworkCore.Auditing/
  sebkuw.ExceptionProcessor/
  sebkuw.QueryableProcessor/
tests/
  sebkuw.Cqrs.Abstractions.Tests/
  sebkuw.Domain.Abstractions.Tests/
  sebkuw.Cqrs.Tests/
  sebkuw.EntityFrameworkCore.Auditing.Tests/
  sebkuw.ExceptionProcessor.Tests/
  sebkuw.QueryableProcessor.Tests/
```

## sebkuw.Domain.Abstractions

### Purpose

`sebkuw.Domain.Abstractions` provides lightweight base types and contracts for domain entities. It is dependency-free and can be referenced from domain or application projects without bringing in infrastructure packages.

### Installation

```bash
dotnet add package sebkuw.Domain.Abstractions
```

### Included Types

- `IEntity<TId>`
- `Entity<TId>`
- `GuidEntity`
- `ICreatedEntity`
- `IUpdatedEntity`
- `CreatedAuditableEntity<TId>`
- `AuditableEntity<TId>`
- `AuditableGuidEntity`
- `IHardDeleteProtected`
- `IDeactivatable`
- `DeactivatableAuditableEntity<TId>`
- `IAppendOnlyEntity`

### Basic Entity

Use `Entity<TId>` when the identifier type is part of your domain design:

```csharp
using sebkuw.Domain.Abstractions.Entities;

public sealed class Product : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}
```

Use `GuidEntity` when your domain entity uses a `Guid` identifier:

```csharp
using sebkuw.Domain.Abstractions.Entities;

public sealed class Customer : GuidEntity
{
    public string Email { get; set; } = string.Empty;
}
```

### Auditable Entity

Use `AuditableEntity<TId>` when an entity should expose creation and update metadata:

```csharp
using sebkuw.Domain.Abstractions.Auditing;

public sealed class Invoice : AuditableEntity<long>
{
    public string Number { get; set; } = string.Empty;
}
```

Use `AuditableGuidEntity` for the common `Guid` identifier case:

```csharp
using sebkuw.Domain.Abstractions.Auditing;

public sealed class Order : AuditableGuidEntity
{
    public string Number { get; set; } = string.Empty;
}
```

Use creation-only auditing for immutable history, and explicit deactivation for entities that must remain available to historical data:

```csharp
using sebkuw.Domain.Abstractions.Auditing;
using sebkuw.Domain.Abstractions.Deactivation;
using sebkuw.Domain.Abstractions.History;

public sealed class PriceHistory : CreatedAuditableEntity<Guid>, IAppendOnlyEntity;

public sealed class Product : DeactivatableAuditableEntity<Guid>;
```

### Notes

- `CreatedBy` and `CreatedAt` represent required creation metadata.
- `UpdatedBy` and `UpdatedAt` are nullable because new entities may not have been updated yet.
- Audit values are intentionally simple so they can be filled by application services, EF Core interceptors, pipeline behaviors or other infrastructure code.

## sebkuw.Cqrs.Abstractions

### Purpose

`sebkuw.Cqrs.Abstractions` defines contracts for commands, queries and handlers. The package does not depend on a mediator library, which keeps application contracts portable and easy to test.

### Installation

```bash
dotnet add package sebkuw.Cqrs.Abstractions
```

### Included Types

- `ICommand`
- `ICommand<TResponse>`
- `IQuery<TResponse>`
- `ICommandHandler<TCommand>`
- `ICommandHandler<TCommand, TResponse>`
- `IQueryHandler<TQuery, TResponse>`

### Commands

Use `ICommand` for operations that do not return a value:

```csharp
using sebkuw.Cqrs.Abstractions;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;

public sealed class DeleteUserCommandHandler
    : ICommandHandler<DeleteUserCommand>
{
    public Task Handle(
        DeleteUserCommand command,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

Use `ICommand<TResponse>` for operations that return a value:

```csharp
using sebkuw.Cqrs.Abstractions;

public sealed record CreateUserCommand(string Email) : ICommand<Guid>;

public sealed class CreateUserCommandHandler
    : ICommandHandler<CreateUserCommand, Guid>
{
    public Task<Guid> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}
```

### Queries

Use `IQuery<TResponse>` for read operations:

```csharp
using sebkuw.Cqrs.Abstractions;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto?>;

public sealed class GetUserByIdQueryHandler
    : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    public Task<UserDto?> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<UserDto?>(null);
    }
}
```

### Notes

- Commands and queries are marker contracts.
- Handlers receive a `CancellationToken` by design.
- The abstractions can be used with a custom dispatcher, a mediator, direct DI resolution or pipeline behaviors.

## sebkuw.Cqrs

### Purpose

`sebkuw.Cqrs` registers CQRS handlers from selected assemblies into `Microsoft.Extensions.DependencyInjection`. It uses Scrutor for assembly scanning.

### Installation

```bash
dotnet add package sebkuw.Cqrs
```

### Register One Assembly

Use the shortcut when handlers live in one assembly:

```csharp
using sebkuw.Cqrs;

builder.Services.AddCqrsFromAssemblyContaining<CreateUserCommandHandler>();
```

### Register Multiple Assemblies

Use the options delegate when handlers are split across modules:

```csharp
using sebkuw.Cqrs;

builder.Services.AddCqrs(options =>
{
    options.RegisterServicesFromAssemblyContaining<CreateUserCommandHandler>();
    options.RegisterServicesFromAssemblyContaining<CreateOrderCommandHandler>();
});
```

### What Gets Registered

The scanner registers implementations of:

- `ICommandHandler<TCommand>`
- `ICommandHandler<TCommand, TResponse>`
- `IQueryHandler<TQuery, TResponse>`

Handlers are registered:

- as implemented interfaces,
- with scoped lifetime,
- from explicitly configured assemblies,
- including non-public handler classes.

### Notes

- `AddCqrs` throws when no assemblies are configured.
- Register handlers in the application composition root, usually where `IServiceCollection` is configured.
- Keep command and query definitions in application modules, then register those modules explicitly.

## sebkuw.EntityFrameworkCore.Auditing

### Purpose

`sebkuw.EntityFrameworkCore.Auditing` fills audit metadata for EF Core entities during `SaveChanges` and `SaveChangesAsync`. It is built around an EF Core `SaveChangesInterceptor` and the auditing contracts from `sebkuw.Domain.Abstractions`.

### Installation

```bash
dotnet add package sebkuw.EntityFrameworkCore.Auditing
```

### Included Types

- `AuditSaveChangesInterceptor`
- `EntityLifecycleInterceptor`
- `HardDeleteNotAllowedException`
- `AppendOnlyEntityModificationException`
- `IDateTimeProvider`
- `ICurrentUserProvider`
- `SystemDateTimeProvider`
- `AddEfCoreAuditing`

### Provide Current User

The package does not assume how your application identifies users. Provide an implementation of `ICurrentUserProvider` in your application layer or infrastructure layer:

```csharp
using sebkuw.EntityFrameworkCore.Auditing.Abstractions;

public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public string GetUserName()
    {
        return "system";
    }
}
```

### Register Auditing Services

Register the current user provider and the auditing services in the composition root:

```csharp
using sebkuw.EntityFrameworkCore.Auditing.Abstractions;
using sebkuw.EntityFrameworkCore.Auditing.Extensions;

builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddEfCoreAuditing();
```

### Attach the Interceptor to DbContext

Resolve `AuditSaveChangesInterceptor` from DI and attach it to your EF Core context:

```csharp
using sebkuw.EntityFrameworkCore.Auditing.Interceptors;

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options
        .UseSqlServer(connectionString)
        .AddSebkuwAuditingInterceptors(serviceProvider);
});
```

### What Gets Updated

For entities implementing `ICreatedEntity`:

- `CreatedBy` and `CreatedAt` are set when the entity is added.
- `CreatedBy` and `CreatedAt` are protected from modification when the entity is updated.

For entities implementing `IUpdatedEntity`:

- `UpdatedBy` and `UpdatedAt` stay empty when the entity is first added.
- `UpdatedBy` and `UpdatedAt` are set when the entity is modified.

The lifecycle interceptor rejects physical deletion of `IHardDeleteProtected` entities and rejects updates and deletes of `IAppendOnlyEntity` records. Deactivation is an explicit domain update; no delete is silently converted and no global active-only query filter is installed.

### Example Entity

```csharp
using sebkuw.Domain.Abstractions.Auditing;

public sealed class Customer : AuditableGuidEntity
{
    public string Email { get; set; } = string.Empty;
}
```

### Notes

- `SystemDateTimeProvider` uses `TimeProvider.System.GetUtcNow()`.
- You can replace `IDateTimeProvider` in tests or applications that need deterministic time.
- The interceptor only touches tracked entities in `Added` or `Modified` state.

## sebkuw.ExceptionProcessor

### Purpose

`sebkuw.ExceptionProcessor` converts exceptions into consistent JSON API responses. It includes a base exception contract for application-specific errors, ready-made exception types, NLog-based exception logging, a global exception middleware and a correlation ID middleware.

### Installation

```bash
dotnet add package sebkuw.ExceptionProcessor
```

### Included Types

- `BaseException`
- `ExceptionResponse`
- `IExceptionManager`
- `ExceptionManager`
- `IExceptionLogger`
- `ExceptionLogger`
- `GlobalExceptionMiddleware`
- `TraceIdMiddleware`
- built-in custom exceptions such as `ValidationException`, `ObjectNotFoundException`, `DatabaseException`, `TimeoutException` and `OperationFailedException`

### Register Services

Register the exception manager and logger in the application composition root:

```csharp
using sebkuw.ExceptionProcessor;
using sebkuw.ExceptionProcessor.Loggers;

builder.Services.AddScoped<IExceptionManager, ExceptionManager>();
builder.Services.AddSingleton<IExceptionLogger, ExceptionLogger>();
```

Initialize the NLog logger from configuration during startup:

```csharp
using sebkuw.ExceptionProcessor.Loggers;

ExceptionLogger.Initialize(builder.Configuration);
```

Example configuration:

```json
{
  "Logging": {
    "NLog": {
      "LogFilePath": "logs/exceptions.log"
    }
  }
}
```

### Add Middleware

Add the correlation ID middleware before the global exception middleware:

```csharp
using sebkuw.ExceptionProcessor.Middlewares;

app.UseMiddleware<TraceIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
```

`TraceIdMiddleware` reads `X-Correlation-ID` from the incoming request when it exists. Otherwise it creates a new ID. The value is stored in `HttpContext.Items["CorrelationId"]` and returned in the `X-Correlation-ID` response header.

### Throw Custom Exceptions

Use the built-in exceptions for common API failures:

```csharp
using sebkuw.ExceptionProcessor.Exceptions.Custom;

throw new ObjectNotFoundException("User", userId);
```

The global middleware returns a response similar to:

```json
{
  "code": "ObjectNotFound",
  "httpCode": 404,
  "mainText": "Object not found",
  "description": "The requested User with ID 123 was not found.",
  "timestamp": "2026-07-06T12:30:00.0000000Z",
  "traceId": "0HN1GH8P91FD0:00000001"
}
```

### Create Application-Specific Exceptions

Derive from `BaseException` when an application needs its own error code and HTTP status:

```csharp
using sebkuw.ExceptionProcessor.Exceptions.Base;

public sealed class DuplicateEmailException : BaseException
{
    public DuplicateEmailException(string email)
        : base(
            "DuplicateEmail",
            409,
            "Duplicate email",
            $"The email '{email}' is already in use.")
    {
    }
}
```

### Notes

- Exceptions derived from `BaseException` keep their configured code, HTTP status, main text and description.
- Unknown exceptions are returned as `UnhandledException` with HTTP 500.
- `ExceptionResponse.Timestamp` is set in UTC when the response is created.
- `GlobalExceptionMiddleware` writes JSON with camel-case property names and includes the correlation ID in both the response body and header.

## sebkuw.QueryableProcessor

### Purpose

`sebkuw.QueryableProcessor` applies filtering, sorting, pagination and projection to `IQueryable<T>` sources. It is useful for API list endpoints that accept query options and should return consistent pagination metadata.

### Installation

```bash
dotnet add package sebkuw.QueryableProcessor
```

### Included Types

- `RequestDto`
- `FilterCondition`
- `FilterOperation`
- `PaginationOptions`
- `PaginationResponse<T>`
- `ApplyFilters`
- `SortBy`
- `Paginate`
- `SolveRequest`

### Request Model

Create a request with optional filters and sorting plus required pagination options:

```csharp
using sebkuw.QueryableProcessor.Enums;
using sebkuw.QueryableProcessor.Models;

var request = new RequestDto
{
    SortParam = "CreatedAt desc",
    Filters =
    [
        new FilterCondition
        {
            PropertyPath = "Status",
            Operation = FilterOperation.Equal,
            Value = "Published"
        },
        new FilterCondition
        {
            PropertyPath = "Name",
            Operation = FilterOperation.Contains,
            Value = "api"
        }
    ],
    PaginationOptions = new PaginationOptions(pageNumber: 1, pageSize: 20)
};
```

### Process an EF Core Query

Use `SolveRequest` to apply filters, sorting, pagination and projection in one call:

```csharp
using sebkuw.QueryableProcessor.Solvers;

PaginationResponse<ArticleListItem> response = await dbContext.Articles
    .SolveRequest(
        request,
        article => new ArticleListItem(
            article.Id,
            article.Title,
            article.CreatedAt),
        cancellationToken);
```

The response contains the page data plus `TotalItems`, `TotalPages`, `PageNumber`, `PageSize`, `HasNextPage` and `HasPreviousPage`.

### Supported Filters

`FilterOperation` supports:

- `Equal`
- `NotEqual`
- `GreaterThan`
- `LessThan`
- `GreaterThanOrEqual`
- `LessThanOrEqual`
- `Contains`
- `StartsWith`
- `EndsWith`
- `In`
- `NotIn`

String operations work on string properties. `In` and `NotIn` expect a collection value, for example `new[] { 1, 2, 3 }`.

### Sorting

Use the Angular-compatible suffix form:

```csharp
query = query.SortBy("Name asc");
query = query.SortBy("Category.Name desc");
```

The legacy `asc_Name` and `desc_Category.Name` forms remain supported. Nested paths and camelCase frontend property names are accepted. If the sort parameter is empty or invalid, the original query is returned unchanged.

### Notes

- `PaginationOptions` defaults to page 1 and page size 10.
- Page numbers are 1-based.
- Page size is limited to 100.
- `SolveRequest` counts matching items before pagination.
- Filters are combined with logical `AND`.

## Build

```bash
dotnet build sebkuw.BuildingBlocks.slnx
```

## Tests

```bash
dotnet test sebkuw.BuildingBlocks.slnx
```

The test projects cover:

- domain entity contracts,
- auditing contracts,
- CQRS handler scanning,
- scoped handler registration,
- validation for missing CQRS assembly configuration,
- EF Core auditing service registration,
- EF Core creation and update audit behavior,
- exception response mapping,
- global exception middleware responses,
- correlation ID middleware behavior,
- dynamic query filtering, sorting and pagination,
- full query request processing with EF Core InMemory.

Coverage is collected with `coverlet.runsettings`. The repository quality gate requires at least 90% line coverage and 80% branch coverage for each production library.

## Project Standards

- [.NET agent instructions](AGENTS.md)
- [Cross-cutting development skill](.agents/skills/develop-dotnet-building-blocks/SKILL.md)
- [Angular shared-components contract](docs/angular-shared-components-contract.md)

The repository targets .NET 10 and C# 14, centralizes package versions, uses locked restore, treats warnings as errors, and admits only approved free open-source licenses. Every behavior change requires proportionate tests, package documentation, and an `Unreleased` changelog entry.

## Changelog

See the [repository changelog](CHANGELOG.md) for cross-cutting changes and each package README for its package-specific history.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
