# sebkuw Building Blocks

[![CI](https://github.com/sebkuw/dotnet-building-blocks/actions/workflows/ci.yml/badge.svg)](https://github.com/sebkuw/dotnet-building-blocks/actions/workflows/ci.yml)

Small, reusable **.NET 10 / C# 14** libraries for domain models, CQRS, EF Core persistence, API errors, and CSV data pipelines.

The packages separate application contracts from infrastructure integrations. Use the pieces your application needs: domain and CQRS abstractions have no runtime package dependencies, while EF Core, ASP.NET Core, and dependency injection integrations live in separate packages.

## Packages

Each package has its own usage guide, tests, version, and changelog.

| Package | What it provides | Runtime integration |
| --- | --- | --- |
| [sebkuw.Domain.Abstractions](src/sebkuw.Domain.Abstractions/README.md) | Entity base types, creation/update audit contracts, explicit deactivation, and append-only/hard-delete markers. | None |
| [sebkuw.Cqrs.Abstractions](src/sebkuw.Cqrs.Abstractions/README.md) | Typed command, query, and handler contracts with cancellation tokens. | None |
| [sebkuw.Cqrs](src/sebkuw.Cqrs/README.md) | Explicit assembly scanning and scoped registration of CQRS handlers. | Microsoft DI, Scrutor |
| [sebkuw.EntityFrameworkCore.Auditing](src/sebkuw.EntityFrameworkCore.Auditing/README.md) | Audit metadata and lifecycle enforcement during tracked `SaveChanges` operations. | EF Core 10, Microsoft DI |
| [sebkuw.ExceptionProcessor](src/sebkuw.ExceptionProcessor/README.md) | Structured API errors, correlation middleware, and replaceable exception logging. | ASP.NET Core 10, bundled NLog adapter |
| [sebkuw.QueryableProcessor](src/sebkuw.QueryableProcessor/README.md) | Expression-based filtering, sorting, pagination, and projected list responses. | EF Core 10 for asynchronous execution |
| [sebkuw.ExportProcessor](src/sebkuw.ExportProcessor/README.md) | Streaming CSV exports with explicit column selection, query criteria, and optional page/row limits. | EF Core 10, QueryableProcessor |
| [sebkuw.ImportProcessor](src/sebkuw.ImportProcessor/README.md) | CSV parsing, typed mapping, validation, preview, ordered batch writes, and idempotency hooks. | None; persistence adapters belong to the application |

All packages target `net10.0`. The code does not require Angular; the query and error JSON contracts also support the companion [angular-shared-components](https://github.com/sebkuw/angular-shared-components) project. See the [HTTP compatibility contract](docs/angular-shared-components-contract.md) for field names and examples.

## Design

- **Contracts stay independent.** Domain and CQRS abstractions do not reference EF Core, HTTP, a DI container, or a mediator.
- **Composition is explicit.** Choose assemblies, attach interceptors, define export columns, and provide import persistence adapters in the application.
- **I/O supports cancellation.** Query execution, CSV processing, and handler contracts accept `CancellationToken`.
- **Consumer concerns stay with the consumer.** Authentication, authorization, transactions, database providers, and domain rules are application responsibilities.

The CQRS integration registers handlers; applications invoke them directly or supply their own dispatcher. Lifecycle markers are enforced by the EF Core interceptors when attached to a context.

## Quick start: CQRS registration

When the package is available in your configured NuGet feed, install it and the DI container implementation:

```powershell
dotnet add package sebkuw.Cqrs
dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.5
```

This complete console example registers a query handler, resolves it within a scope, and invokes it through its public contract:

```csharp
using Microsoft.Extensions.DependencyInjection;
using sebkuw.Cqrs;
using sebkuw.Cqrs.Abstractions;

var services = new ServiceCollection();
services.AddCqrsFromAssemblyContaining<GreetingHandler>();

using var provider = services.BuildServiceProvider(validateScopes: true);
using var scope = provider.CreateScope();

var handler = scope.ServiceProvider
    .GetRequiredService<IQueryHandler<GreetingQuery, string>>();

Console.WriteLine(await handler.Handle(new GreetingQuery("World"), CancellationToken.None));

public sealed record GreetingQuery(string Name) : IQuery<string>;

public sealed class GreetingHandler : IQueryHandler<GreetingQuery, string>
{
    public Task<string> Handle(GreetingQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"Hello, {query.Name}!");
    }
}
```

For persistence and HTTP examples, start with the [auditing](src/sebkuw.EntityFrameworkCore.Auditing/README.md), [query processing](src/sebkuw.QueryableProcessor/README.md), or [API error handling](src/sebkuw.ExceptionProcessor/README.md) guide.

## Build from source

Prerequisites: the **.NET SDK 10.0.400 feature band** (latest patch permitted by `global.json`) and **PowerShell 7** for the verification scripts. Runtime dependencies restore from `nuget.org` without GitHub credentials.

```powershell
git clone https://github.com/sebkuw/dotnet-building-blocks.git
cd dotnet-building-blocks
dotnet restore sebkuw.BuildingBlocks.slnx --locked-mode
dotnet build sebkuw.BuildingBlocks.slnx --configuration Release --no-restore
dotnet test sebkuw.BuildingBlocks.slnx --configuration Release --no-build
```

Package installation depends on publication to your configured feed. To use the current checkout without a published release, build the local packages:

```powershell
dotnet pack sebkuw.BuildingBlocks.slnx --configuration Release --no-build --output artifacts/packages
```

In a separate consumer project, configure the generated directory as a package source alongside `nuget.org`. This example `NuGet.Config` routes `sebkuw.*` to the local packages and external dependencies to the public feed; replace the local path with your checkout path:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-sebkuw" value="C:\repos\dotnet-building-blocks\artifacts\packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local-sebkuw">
      <package pattern="sebkuw.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

## Verification

The [CI workflow](.github/workflows/ci.yml) runs locked dependency restore, license verification, formatting, a Release build, tests with coverage, and package-content verification. NuGet vulnerability auditing is enabled for direct and transitive dependencies, and reported vulnerability warnings fail the build.

Run the same checks locally from the repository root:

```powershell
dotnet restore sebkuw.BuildingBlocks.slnx --locked-mode
./eng/verify-package-licenses.ps1
dotnet format sebkuw.BuildingBlocks.slnx --verify-no-changes --no-restore
dotnet build sebkuw.BuildingBlocks.slnx --configuration Release --no-restore
dotnet test sebkuw.BuildingBlocks.slnx --configuration Release --no-build --settings coverlet.runsettings --collect:"XPlat Code Coverage" --results-directory artifacts/TestResults
./eng/verify-coverage.ps1
dotnet pack sebkuw.BuildingBlocks.slnx --configuration Release --no-build --output artifacts/packages
./eng/verify-packages.ps1
```

The package checker expects one version of each package. If you retain earlier build outputs, pack into a fresh directory and pass it as `-PackageDirectory` to `verify-packages.ps1`. Likewise, pass the current test run's directory as `-ResultsDirectory` to `verify-coverage.ps1` to avoid mixing coverage from different runs.

The coverage gate requires **90% line coverage and 80% branch coverage per production library**. Tests cover public contracts, DI registration, auditing/lifecycle rules, middleware and JSON responses, query operations, CSV formatting, import preview and batching, and cancellation. EF Core integration tests currently use **InMemory**; they do not establish SQL translation or behavior for every relational provider. Validate query expressions against the provider used by your application.

Generated NuGet packages contain their assembly, XML API documentation, README, changelog, and MIT license metadata. Build outputs and coverage reports are ignored by Git.

## Limitations

- Dynamic query paths resolve public properties. Applications should restrict permitted filter/sort fields and apply authorization before processing a request. Use a stable sort for pagination.
- Auditing and lifecycle rules apply to tracked `SaveChanges` operations. Bulk operations, direct SQL, and external writers need separate enforcement.
- CSV import is streaming, but defaults retain all row reports and buffer valid values for a single write. Configure finite batch size and report retention for large files; individual records and idempotency keys also consume memory.
- CSV writes are incremental. Applications own transactions for imports and handling of partially written export responses.
- Custom API exceptions carry client-visible descriptions. Supply only safe display text; unexpected exception messages are hidden from clients.

## Repository layout

```text
src/       Eight independently packaged libraries and their usage guides
tests/     Corresponding xUnit unit, integration, and contract test projects
docs/      Angular/.NET HTTP compatibility contract
eng/       License, coverage, and NuGet package verification scripts
.github/   Continuous integration workflow
```

## Publishing to GitHub Packages

The manually triggered `Publish GitHub Packages` workflow publishes every `sebkuw.*` package present at an immutable commit from the default-branch history. Its default source is commit `9aefe4b63b82a15cbbec5c97767a9788852e41f3`, where the seven packages consumed by Meblicz are version `2.0.0`. Later releases must supply a full commit SHA from `main`; package versions come only from the projects at that commit.

The workflow checks out release tooling separately from the selected source, restores locked dependencies, runs every quality gate from that source revision, verifies each packed version and repository commit against it, and then publishes to the `sebkuw` GitHub Packages NuGet feed. This makes a rerun select the same source and package versions rather than relabeling current code as an older release.

Publishing uses only the workflow-scoped `GITHUB_TOKEN`, with `contents: read` and `packages: write`. No personal access token or stored package credential is required. Package metadata includes `https://github.com/sebkuw/dotnet-building-blocks` as the source repository, so GitHub can connect each package to this repository and expose granular package settings.

After the first publication, grant the consumer repository access separately for each package:

1. Open the package on the `sebkuw` GitHub profile and select **Package settings**.
2. Under **Manage Actions access**, add `sebkuw/meblicz` with the **Read** role.
3. Repeat for every `sebkuw.*` package consumed by Meblicz.

The Meblicz workflow can then restore the private packages with its own workflow-scoped `GITHUB_TOKEN` and `packages: read`; no personal access token is needed. Keep credentials out of committed `NuGet.Config` files and logs.

## Contributing and versioning

Read [AGENTS.md](AGENTS.md) and the instructions for the affected library before changing code. Keep public API changes, tests, documentation, and changelog entries together. Dependencies use central version management and committed lock files; nullable checks, analyzers, and warnings as errors are enabled.

Versions follow Semantic Versioning per package. The `sebkuw.*` rename is a breaking migration for existing consumers: replace the previous package IDs and namespace imports, and use `AddSebkuwAuditingInterceptors()` for the auditing integration. Package-specific details are recorded in the [changelogs](CHANGELOG.md).

## License

[MIT](LICENSE) — Copyright (c) 2026 Sebastian Wnorowski.
