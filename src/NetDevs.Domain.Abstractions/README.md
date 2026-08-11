# NetDevs.Domain.Abstractions

Dependency-free .NET 10 contracts and base types for domain entities and audit metadata.

## Installation

```powershell
dotnet add package NetDevs.Domain.Abstractions
```

## Public API

- `IEntity<TId>` and `Entity<TId>`
- `GuidEntity`
- `ICreatedEntity` and `IUpdatedEntity`
- `AuditableEntity<TId>` and `AuditableGuidEntity`

## Usage

```csharp
using NetDevs.Domain.Abstractions.Auditing;

public sealed class Order : AuditableGuidEntity
{
    public string Number { get; set; } = string.Empty;
}
```

`CreatedBy` and `CreatedAt` describe creation. `UpdatedBy` and `UpdatedAt` are nullable until the first update. The package does not generate identifiers, read the current user, access a clock, or depend on persistence infrastructure.

## Compatibility

The auditing interfaces are consumed by `NetDevs.EntityFrameworkCore.Auditing`. Property types, nullability, mutability, generic constraints, and inheritance are public API and follow Semantic Versioning.

## Development

- [Library instructions](AGENTS.md)
- [Development skill](../../.agents/skills/develop-netdevs-domain-abstractions/SKILL.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/NetDevs.Domain.Abstractions.Tests`

```powershell
dotnet test tests/NetDevs.Domain.Abstractions.Tests/NetDevs.Domain.Abstractions.Tests.csproj
```
