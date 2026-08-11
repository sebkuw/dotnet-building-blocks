# NetDevs.EntityFrameworkCore.Auditing

.NET 10 and EF Core 10 auditing based on a reusable `SaveChangesInterceptor`.

## Installation

```powershell
dotnet add package NetDevs.EntityFrameworkCore.Auditing
```

## Configure the current user

```csharp
using NetDevs.EntityFrameworkCore.Auditing.Abstractions;

public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public string GetUserName() => "system";
}
```

## Register and attach the interceptor

```csharp
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddEfCoreAuditing();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options
        .UseSqlServer(connectionString)
        .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
});
```

For added entities implementing `ICreatedEntity`, the interceptor sets creation user and UTC time. For modified entities implementing `IUpdatedEntity`, it sets update user and UTC time while protecting original creation metadata. Replace `IDateTimeProvider` for deterministic time.

## Design constraints

The package knows only the generic audit contracts from `NetDevs.Domain.Abstractions`. It does not depend on HTTP, a particular identity provider, or a concrete `DbContext`.

## Development

- [Library instructions](AGENTS.md)
- [Development skill](../../.agents/skills/develop-netdevs-efcore-auditing/SKILL.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/NetDevs.EntityFrameworkCore.Auditing.Tests`

```powershell
dotnet test tests/NetDevs.EntityFrameworkCore.Auditing.Tests/NetDevs.EntityFrameworkCore.Auditing.Tests.csproj
```
