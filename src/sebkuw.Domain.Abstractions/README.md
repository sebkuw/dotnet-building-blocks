# sebkuw.Domain.Abstractions

Dependency-free .NET 10 contracts and base types for domain entities, audit metadata, deactivation, and persistence lifecycle markers.

## Installation

```powershell
dotnet add package sebkuw.Domain.Abstractions
```

## Public API

- `IEntity<TId>` and `Entity<TId>`
- `GuidEntity`
- `ICreatedEntity` and `IUpdatedEntity`
- `CreatedAuditableEntity<TId>`
- `AuditableEntity<TId>` and `AuditableGuidEntity`
- `IHardDeleteProtected`
- `IDeactivatable` and `DeactivatableAuditableEntity<TId>`
- `IAppendOnlyEntity`

## Usage

```csharp
using sebkuw.Domain.Abstractions.Auditing;

public sealed class Order : AuditableGuidEntity
{
    public string Number { get; set; } = string.Empty;
}
```

`CreatedBy` and `CreatedAt` describe creation. `UpdatedBy` and `UpdatedAt` are nullable until the first update. The package does not generate identifiers, read the current user, access a clock, or depend on persistence infrastructure.

Use `CreatedAuditableEntity<TId>` for records that only need creation metadata, especially append-only history:

```csharp
using sebkuw.Domain.Abstractions.Auditing;
using sebkuw.Domain.Abstractions.History;

public sealed class SupplierPriceHistory
    : CreatedAuditableEntity<Guid>, IAppendOnlyEntity
{
    public decimal Price { get; set; }
}
```

Use `DeactivatableAuditableEntity<TId>` when an entity should leave active use without losing historical references:

```csharp
using sebkuw.Domain.Abstractions.Deactivation;

var product = new Product();
product.Deactivate(Guid.NewGuid(), TimeProvider.System.GetUtcNow());
product.Reactivate();

public sealed class Product : DeactivatableAuditableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
}

```

New deactivatable entities are active. Deactivation requires a non-empty user ID and a UTC timestamp. Repeating it while inactive preserves the current deactivation metadata; reactivation clears that metadata. `IDeactivatable` and `IAppendOnlyEntity` both implement `IHardDeleteProtected`.

`Deactivate` rejects `Guid.Empty` with `ArgumentOutOfRangeException` and a non-UTC timestamp with `ArgumentException`. Audit properties use `DateTimeOffset`; `CreatedBy` is a string, while `DeactivatedBy` is a nullable `Guid`. Applications must assign identifiers and creation metadata before persistence; declaring `CreatedBy` non-nullable does not populate it automatically.

## Compatibility

The auditing and lifecycle interfaces are consumed by `sebkuw.EntityFrameworkCore.Auditing`. Property types, nullability, mutability, generic constraints, and inheritance are public API and follow Semantic Versioning. `AuditableEntity<TId>` still exposes the same creation and update properties; creation properties are now inherited from `CreatedAuditableEntity<TId>`.

## Limitations

This package defines domain contracts only. It does not enforce persistence rules, install query filters, publish domain events, or retain a history of activation changes. Use the EF Core integration package to enforce append-only and hard-delete rules. Applications may opt into their own `IsActive` query filter and bypass it explicitly with `IgnoreQueryFilters()` for administration or history queries.

## Development

Run the test command from the repository root. Full verification instructions are in the [repository guide](https://github.com/sebkuw/dotnet-building-blocks#verification).

- [Changelog](https://github.com/sebkuw/dotnet-building-blocks/blob/main/src/sebkuw.Domain.Abstractions/CHANGELOG.md)
- Tests: `tests/sebkuw.Domain.Abstractions.Tests`

```powershell
dotnet test tests/sebkuw.Domain.Abstractions.Tests/sebkuw.Domain.Abstractions.Tests.csproj
```
