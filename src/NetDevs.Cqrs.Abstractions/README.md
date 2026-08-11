# NetDevs.Cqrs.Abstractions

Dependency-free .NET 10 contracts for commands, queries, and strongly typed handlers.

## Installation

```powershell
dotnet add package NetDevs.Cqrs.Abstractions
```

## Public API

- `ICommand` and `ICommand<TResponse>`
- `IQuery<TResponse>`
- `ICommandHandler<TCommand>`
- `ICommandHandler<TCommand, TResponse>`
- `IQueryHandler<TQuery, TResponse>`

## Usage

```csharp
using NetDevs.Cqrs.Abstractions;

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

`NetDevs.Cqrs` scans these handler interfaces. Generic constraints, variance, return types, and cancellation semantics are versioned public API.

## Development

- [Library instructions](AGENTS.md)
- [Development skill](../../.agents/skills/develop-netdevs-cqrs-abstractions/SKILL.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/NetDevs.Cqrs.Abstractions.Tests`

```powershell
dotnet test tests/NetDevs.Cqrs.Abstractions.Tests/NetDevs.Cqrs.Abstractions.Tests.csproj
```
