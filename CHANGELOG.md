# Changelog

All notable repository-wide changes are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html) per package.

## [Unreleased]

### Added

- The new `sebkuw.ExportProcessor` package for streaming CSV exports from `IQueryable<T>` with `RequestDto` filtering, sorting, optional pagination, explicit selectable columns, row limits, cancellation, and spreadsheet formula protection.
- A least-privilege, manually triggered GitHub Actions workflow that publishes the project-declared `sebkuw.*` package versions from an immutable default-branch commit to GitHub Packages with the repository-scoped `GITHUB_TOKEN`.
- Package verification for the exact source-revision package set, project-declared release versions, source-repository URL, and repository commit required for deterministic releases and granular GitHub Packages permissions.
- The new `sebkuw.ImportProcessor` package for streaming, domain-neutral CSV imports with mapping, conversion, validation, preview, batch reporting, application-owned persistence, and idempotency extension points.
- Reusable entity lifecycle contracts for hard-delete protection, deactivation, and append-only records.
- EF Core lifecycle enforcement integrated with the existing auditing registration pipeline.
- Hierarchical `AGENTS.md` guidance for the repository, tests, and every production library.
- Repository-local development skills for cross-cutting work and all six NuGet packages.
- Per-package README and changelog files included in NuGet packages.
- A documented compatibility contract with `angular-shared-components`.
- CI quality gates for formatting, Release build, tests, coverage, licenses, vulnerabilities, packing, and package contents.
- The missing `sebkuw.Cqrs.Abstractions.Tests` project.

### Changed

- Organized the ImportProcessor and ExportProcessor source trees by responsibility while preserving their public namespaces and APIs.
- Added bounded concurrent mapping, ordered batch writes, and bounded row-report retention for large CSV imports; advanced `sebkuw.ImportProcessor` and `sebkuw.QueryableProcessor` to version 2.1.0 for their additive APIs.
- Enforce the repository-wide C# style of omitting braces from single-statement `if` bodies.
- Advanced the pinned .NET 10 SDK feature band to 10.0.400 so patched .NET tooling can be selected.
- Updated the existing build-only Source Link dependency to the patched .NET 10 line after the previous transitive Git build task was flagged by NuGet audit.
- Renamed every library, NuGet package, assembly, namespace, project, test project, solution reference, and repository development skill from the previous package prefix to `sebkuw`.
- Advanced all packages to version 2.0.0 because consumers must replace their package references and namespace imports with the corresponding `sebkuw.*` names.
- Split creation-only audit metadata into `CreatedAuditableEntity<TId>` while preserving the public `AuditableEntity<TId>` contract.
- Standardized the repository on the .NET 10 SDK feature band and C# 14.
- Centralized NuGet package versions and enabled deterministic locked restore.
- Scoped dependency restore to the public `nuget.org` source with explicit source mapping.
- Enabled nullable analysis, recommended .NET analyzers, warnings as errors, Source Link, and deterministic builds.
- Declared the repository's MIT license in every NuGet package and added package-content validation for it.
- Localized all repository and package `AGENTS.md` instructions, development skills, and skill interface metadata into Polish.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/HEAD...HEAD
