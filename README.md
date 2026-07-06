# NetDevs Building Blocks

Reusable .NET building blocks for Clean Architecture applications.

This repository contains small, focused packages that keep application and domain code independent from infrastructure concerns. The current packages cover domain entities, auditing contracts, CQRS contracts and CQRS handler registration for Microsoft dependency injection.

## Table of Contents

- [Packages](#packages)
- [Repository Structure](#repository-structure)
- [NetDevs.Domain.Abstractions](#netdevsdomainabstractions)
- [NetDevs.Cqrs.Abstractions](#netdevscqrsabstractions)
- [NetDevs.Cqrs](#netdevscqrs)
- [Build](#build)
- [Tests](#tests)
- [License](#license)

## Packages

| Package | Purpose |
| --- | --- |
| `NetDevs.Domain.Abstractions` | Base entity and auditing contracts for domain models. |
| `NetDevs.Cqrs.Abstractions` | Dependency-free CQRS contracts for commands, queries and handlers. |
| `NetDevs.Cqrs` | Automatic CQRS handler registration for `Microsoft.Extensions.DependencyInjection` using Scrutor. |

## Repository Structure

```text
src/
  NetDevs.Domain.Abstractions/
  NetDevs.Cqrs.Abstractions/
  NetDevs.Cqrs/
tests/
  NetDevs.Domain.Abstractions.Tests/
  NetDevs.Cqrs.Tests/
```

## NetDevs.Domain.Abstractions

### Purpose

`NetDevs.Domain.Abstractions` provides lightweight base types and contracts for domain entities. It is dependency-free and can be referenced from domain or application projects without bringing in infrastructure packages.

### Installation

```bash
dotnet add package NetDevs.Domain.Abstractions
```

### Included Types

- `IEntity<TId>`
- `Entity<TId>`
- `GuidEntity`
- `ICreatedEntity`
- `IUpdatedEntity`
- `AuditableEntity<TId>`
- `AuditableGuidEntity`

### Basic Entity

Use `Entity<TId>` when the identifier type is part of your domain design:

```csharp
using NetDevs.Domain.Abstractions.Entities;

public sealed class Product : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}
```

Use `GuidEntity` when your domain entity uses a `Guid` identifier:

```csharp
using NetDevs.Domain.Abstractions.Entities;

public sealed class Customer : GuidEntity
{
    public string Email { get; set; } = string.Empty;
}
```

### Auditable Entity

Use `AuditableEntity<TId>` when an entity should expose creation and update metadata:

```csharp
using NetDevs.Domain.Abstractions.Auditing;

public sealed class Invoice : AuditableEntity<long>
{
    public string Number { get; set; } = string.Empty;
}
```

Use `AuditableGuidEntity` for the common `Guid` identifier case:

```csharp
using NetDevs.Domain.Abstractions.Auditing;

public sealed class Order : AuditableGuidEntity
{
    public string Number { get; set; } = string.Empty;
}
```

### Notes

- `CreatedBy` and `CreatedAt` represent required creation metadata.
- `UpdatedBy` and `UpdatedAt` are nullable because new entities may not have been updated yet.
- Audit values are intentionally simple so they can be filled by application services, EF Core interceptors, pipeline behaviors or other infrastructure code.

## NetDevs.Cqrs.Abstractions

### Purpose

`NetDevs.Cqrs.Abstractions` defines contracts for commands, queries and handlers. The package does not depend on a mediator library, which keeps application contracts portable and easy to test.

### Installation

```bash
dotnet add package NetDevs.Cqrs.Abstractions
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
using NetDevs.Cqrs.Abstractions;

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
using NetDevs.Cqrs.Abstractions;

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
using NetDevs.Cqrs.Abstractions;

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

## NetDevs.Cqrs

### Purpose

`NetDevs.Cqrs` registers CQRS handlers from selected assemblies into `Microsoft.Extensions.DependencyInjection`. It uses Scrutor for assembly scanning.

### Installation

```bash
dotnet add package NetDevs.Cqrs
```

### Register One Assembly

Use the shortcut when handlers live in one assembly:

```csharp
using NetDevs.Cqrs;

builder.Services.AddCqrsFromAssemblyContaining<CreateUserCommandHandler>();
```

### Register Multiple Assemblies

Use the options delegate when handlers are split across modules:

```csharp
using NetDevs.Cqrs;

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

## Build

```bash
dotnet build NetDevs.BuildingBlocks.slnx
```

## Tests

```bash
dotnet test NetDevs.BuildingBlocks.slnx
```

The test projects cover:

- domain entity contracts,
- auditing contracts,
- CQRS handler scanning,
- scoped handler registration,
- validation for missing CQRS assembly configuration.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
