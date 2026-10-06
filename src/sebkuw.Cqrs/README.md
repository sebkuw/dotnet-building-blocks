# sebkuw.Cqrs

.NET 10 integration that registers sebkuw CQRS handlers in `Microsoft.Extensions.DependencyInjection` through explicit assembly scanning.

## Installation

```powershell
dotnet add package sebkuw.Cqrs
```

## Registration

Register one assembly:

```csharp
using sebkuw.Cqrs;

services.AddCqrsFromAssemblyContaining<CreateOrderHandler>();
```

Register multiple assemblies:

```csharp
services.AddCqrs(options =>
{
    options.RegisterServicesFromAssemblyContaining<CreateOrderHandler>();
    options.RegisterServicesFromAssemblyContaining<CreateInvoiceHandler>();
});
```

The scanner registers implementations of `ICommandHandler<TCommand>`, `ICommandHandler<TCommand,TResponse>`, and `IQueryHandler<TQuery,TResponse>` as implemented interfaces with scoped lifetime, including non-public handler classes. Repeated assembly selections within one call are deduplicated. At least one assembly must be configured; otherwise `AddCqrs` throws `InvalidOperationException`. Null arguments are rejected with `ArgumentNullException`.

The examples assume an existing `IServiceCollection` and application-owned handler types. Resolve handlers inside a DI scope and invoke their `Handle` method through a CQRS interface. See the [complete console example](https://github.com/sebkuw/dotnet-building-blocks#quick-start-cqrs-registration).

## Design constraints

The package performs registration only. It does not dispatch messages or add behaviors, validation, retries, transactions, or application-specific conventions.

`AsImplementedInterfaces()` registers all implemented interfaces on matched classes, including interfaces outside CQRS. Separate registration calls may add duplicate descriptors; configure the application's assemblies together. The package uses Scrutor and Microsoft DI abstractions; a standalone console application also needs a DI container implementation such as `Microsoft.Extensions.DependencyInjection`.

## Development

Run the test command from the repository root. Full verification instructions are in the [repository guide](https://github.com/sebkuw/dotnet-building-blocks#verification).

- [Changelog](https://github.com/sebkuw/dotnet-building-blocks/blob/main/src/sebkuw.Cqrs/CHANGELOG.md)
- Tests: `tests/sebkuw.Cqrs.Tests`

```powershell
dotnet test tests/sebkuw.Cqrs.Tests/sebkuw.Cqrs.Tests.csproj
```
