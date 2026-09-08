# sebkuw.EntityFrameworkCore.Auditing

.NET 10 and EF Core 10 auditing and entity lifecycle enforcement based on reusable `SaveChangesInterceptor` implementations.

## Installation

```powershell
dotnet add package sebkuw.EntityFrameworkCore.Auditing
```

## Configure the current user

```csharp
using sebkuw.EntityFrameworkCore.Auditing.Abstractions;

public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public string GetUserName() => "system";
}
```

## Register and attach the interceptors

```csharp
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddEfCoreAuditing();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options
        .UseSqlServer(connectionString)
        .AddSebkuwAuditingInterceptors(serviceProvider);
});
```

`AddEfCoreAuditing()` registers both `AuditSaveChangesInterceptor` and `EntityLifecycleInterceptor`. `AddSebkuwAuditingInterceptors()` attaches them in lifecycle-first order so invalid writes are rejected before audit values are applied.

For added entities implementing `ICreatedEntity`, including `CreatedAuditableEntity<TId>`, the audit interceptor sets creation user and UTC time. For modified entities implementing `IUpdatedEntity`, it sets update user and UTC time while protecting original creation metadata. Replace `IDateTimeProvider` for deterministic time.

## Lifecycle enforcement

- Deleting an `IHardDeleteProtected` entity throws `HardDeleteNotAllowedException`.
- Modifying an `IAppendOnlyEntity` throws `AppendOnlyEntityModificationException`.
- Adding an append-only entity is allowed and its creation audit values are populated.
- Deactivating an `IDeactivatable` entity is a normal update, so update audit values are populated.
- Deleting an entity without a lifecycle marker retains the normal EF Core behavior.

The interceptor does not silently convert deletes into deactivation, add global `IsActive` query filters, or publish domain events. Deactivation must be explicit in domain code. Applications that add an optional global query filter can use EF Core's `IgnoreQueryFilters()` for administrative and historical queries.

## Design constraints

The package knows only the generic audit and lifecycle contracts from `sebkuw.Domain.Abstractions`. It does not depend on HTTP, a particular identity provider, or a concrete `DbContext`. Lifecycle enforcement applies to EF Core's standard tracked `SaveChanges` pipeline; direct SQL and external database writers require database-level controls when equivalent enforcement is needed.

## Development

- [Library instructions](AGENTS.md)
- [Development skill](../../.agents/skills/develop-sebkuw-efcore-auditing/SKILL.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/sebkuw.EntityFrameworkCore.Auditing.Tests`

```powershell
dotnet test tests/sebkuw.EntityFrameworkCore.Auditing.Tests/sebkuw.EntityFrameworkCore.Auditing.Tests.csproj
```
