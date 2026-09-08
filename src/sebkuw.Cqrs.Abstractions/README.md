# sebkuw.Cqrs.Abstractions

Dependency-free .NET 10 contracts for commands, queries, and strongly typed handlers.

## Installation

```powershell
dotnet add package sebkuw.Cqrs.Abstractions
```

## Public API

- `ICommand` and `ICommand<TResponse>`
- `IQuery<TResponse>`
- `ICommandHandler<TCommand>`
- `ICommandHandler<TCommand, TResponse>`
- `IQueryHandler<TQuery, TResponse>`

## Usage

```csharp
using sebkuw.Cqrs.Abstractions;

public sealed record CreateOrder(string Number) : ICommand<Guid>;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Guid>
{
    public Task<Guid> Handle(CreateOrder command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}
```

The package does not prescribe a mediator, dispatcher, DI container, validation pipeline, or application architecture. Every handler accepts a `CancellationToken`.

## Compatibility

`sebkuw.Cqrs` scans these handler interfaces. Generic constraints, variance, return types, and cancellation semantics are versioned public API.

## Development

- [Library instructions](AGENTS.md)
- [Development skill](../../.agents/skills/develop-sebkuw-cqrs-abstractions/SKILL.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/sebkuw.Cqrs.Abstractions.Tests`

```powershell
dotnet test tests/sebkuw.Cqrs.Abstractions.Tests/sebkuw.Cqrs.Abstractions.Tests.csproj
```
