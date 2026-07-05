# NetDevs Building Blocks

Reusable .NET building blocks for Clean Architecture applications.

This repository contains small, focused packages that help keep application code independent from infrastructure concerns. The current packages provide CQRS marker interfaces and dependency injection registration for command and query handlers.

## Packages

| Package | Purpose |
| --- | --- |
| `NetDevs.Cqrs.Abstractions` | Dependency-free CQRS contracts for commands, queries and handlers. |
| `NetDevs.Cqrs` | Automatic CQRS handler registration for `Microsoft.Extensions.DependencyInjection` using Scrutor. |

## Repository Structure

```text
src/
  NetDevs.Cqrs.Abstractions/
  NetDevs.Cqrs/
tests/
  NetDevs.Cqrs.Tests/
```

## Installation

Install the abstractions package in projects that define commands, queries or handlers:

```bash
dotnet add package NetDevs.Cqrs.Abstractions
```

Install the registration package in the application composition root:

```bash
dotnet add package NetDevs.Cqrs
```

## CQRS Abstractions

Define commands and queries by implementing the marker interfaces:

```csharp
using NetDevs.Cqrs.Abstractions;

public sealed record CreateUserCommand(string Email) : ICommand<Guid>;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto?>;
```

Implement handlers with the matching handler interfaces:

```csharp
using NetDevs.Cqrs.Abstractions;

public sealed class CreateUserCommandHandler
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        // Application logic goes here.
        return Guid.NewGuid();
    }
}

public sealed class GetUserByIdQueryHandler
    : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        // Query logic goes here.
        return null;
    }
}
```

Commands that do not return a value can implement `ICommand`:

```csharp
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

## Handler Registration

Register CQRS handlers from one or more assemblies:

```csharp
using NetDevs.Cqrs;

builder.Services.AddCqrs(options =>
{
    options.RegisterServicesFromAssemblyContaining<CreateUserCommandHandler>();
});
```

For the common case where all handlers live in one application assembly, use the shortcut:

```csharp
builder.Services.AddCqrsFromAssemblyContaining<CreateUserCommandHandler>();
```

The registration package scans the selected assemblies for:

- `ICommandHandler<TCommand>`
- `ICommandHandler<TCommand, TResponse>`
- `IQueryHandler<TQuery, TResponse>`

Handlers are registered as scoped services and exposed through their implemented interfaces. Internal handler classes are supported.

## Design Goals

- Keep application contracts lightweight and dependency-free.
- Make handler registration explicit at the composition root.
- Avoid coupling command and query definitions to a mediator implementation.
- Keep the packages small enough to use in modular monoliths, Clean Architecture solutions and service-oriented applications.

## Building

```bash
dotnet build NetDevs.BuildingBlocks.slnx
```

## Testing

```bash
dotnet test NetDevs.BuildingBlocks.slnx
```

The test project covers handler scanning, scoped registration and validation for missing assembly configuration.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
