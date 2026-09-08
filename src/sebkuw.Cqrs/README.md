# sebkuw.Cqrs

.NET 10 integration that registers sebkuw CQRS handlers in `Microsoft.Extensions.DependencyInjection` through explicit assembly scanning.

## Installation

```powershell
dotnet add package sebkuw.Cqrs
```

## Registration

Register one assembly:

```csharp
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

The scanner registers implementations of `ICommandHandler<TCommand>`, `ICommandHandler<TCommand,TResponse>`, and `IQueryHandler<TQuery,TResponse>` as implemented interfaces with scoped lifetime. At least one assembly must be configured.

## Design constraints

The package performs registration only. It does not dispatch messages or add behaviors, validation, retries, transactions, or application-specific conventions.

## Development

- [Library instructions](AGENTS.md)
- [Development skill](../../.agents/skills/develop-sebkuw-cqrs/SKILL.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/sebkuw.Cqrs.Tests`

```powershell
dotnet test tests/sebkuw.Cqrs.Tests/sebkuw.Cqrs.Tests.csproj
```
