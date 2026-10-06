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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sebkuw.EntityFrameworkCore.Auditing.Abstractions;
using sebkuw.EntityFrameworkCore.Auditing.Extensions;

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

The example assumes an application-owned `AppDbContext`, a connection string, and a separately installed EF Core SQL Server provider. Choose the provider used by your application; this package does not install one. Audit interception is scoped, lifecycle interception and the default time provider are singletons. Register a custom `IDateTimeProvider` **after** `AddEfCoreAuditing()` to replace the default for single-service resolution.

For added entities implementing `ICreatedEntity`, including `CreatedAuditableEntity<TId>`, the audit interceptor sets creation user and UTC time. For modified entities implementing `IUpdatedEntity`, it sets update user and UTC time while protecting original creation metadata. Replace `IDateTimeProvider` for deterministic time.

Both synchronous and asynchronous saves are supported. Added entities do not receive generated update metadata; caller-supplied update values are not cleared. Creation-field protection prevents database updates to those fields; it does not reset changed values on the tracked object. Current-user and time providers must return application-approved user values and UTC timestamps.

## Lifecycle enforcement

- Deleting an `IHardDeleteProtected` entity throws `HardDeleteNotAllowedException`.
- Modifying an `IAppendOnlyEntity` throws `AppendOnlyEntityModificationException`.
- Adding an append-only entity is allowed and its creation audit values are populated.
- Deactivating an `IDeactivatable` entity is a normal update, so update audit values are populated.
- Deleting an entity without a lifecycle marker retains the normal EF Core behavior.

The interceptor does not silently convert deletes into deactivation, add global `IsActive` query filters, or publish domain events. Deactivation must be explicit in domain code. Applications that add an optional global query filter can use EF Core's `IgnoreQueryFilters()` for administrative and historical queries.

## Design constraints

The package knows only the generic audit and lifecycle contracts from `sebkuw.Domain.Abstractions`. It does not depend on HTTP, a particular identity provider, or a concrete `DbContext`. Lifecycle enforcement applies to EF Core's standard tracked `SaveChanges` pipeline; `ExecuteUpdate`, `ExecuteDelete`, direct SQL, and external database writers require separate controls when equivalent enforcement is needed.

## Development

Run the test command from the repository root. Full verification instructions are in the [repository guide](https://github.com/sebkuw/dotnet-building-blocks#verification).

- [Changelog](https://github.com/sebkuw/dotnet-building-blocks/blob/main/src/sebkuw.EntityFrameworkCore.Auditing/CHANGELOG.md)
- Tests: `tests/sebkuw.EntityFrameworkCore.Auditing.Tests`

```powershell
dotnet test tests/sebkuw.EntityFrameworkCore.Auditing.Tests/sebkuw.EntityFrameworkCore.Auditing.Tests.csproj
```
