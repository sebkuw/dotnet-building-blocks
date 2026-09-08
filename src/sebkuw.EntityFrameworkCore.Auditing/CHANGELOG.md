# Changelog

All notable changes to `sebkuw.EntityFrameworkCore.Auditing` are documented here. The format follows Keep a Changelog and Semantic Versioning.

## [Unreleased]

### Added

- Package-specific documentation, development instructions, changelog, and quality workflow.
- `EntityLifecycleInterceptor` enforcing hard-delete protection and append-only updates for tracked entities.
- `HardDeleteNotAllowedException` and `AppendOnlyEntityModificationException`.
- `AddSebkuwAuditingInterceptors()` for attaching the complete auditing and lifecycle pipeline to a `DbContext`.

### Changed

- Renamed the package, assembly, project, public namespaces, and auditing extension prefix to `sebkuw`; version 2.0.0 requires consumers to update package references, imports, and the interceptor registration call.
- Centralized EF Core and dependency-injection package versions.
- Declared the package's MIT license in NuGet metadata.
- `AddEfCoreAuditing()` now registers the lifecycle interceptor in addition to the audit interceptor.
- Advanced the package version to `1.1.0` for the additive public API.

## [1.0.0] - 2026-08-11

### Added

- SaveChanges interceptor for creation and update audit metadata.
- Replaceable current-user and UTC-time providers with DI registration.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/sebkuw.EntityFrameworkCore.Auditing-v1.0.0...HEAD
[1.0.0]: https://github.com/sebkuw/dotnet-building-blocks/releases/tag/sebkuw.EntityFrameworkCore.Auditing-v1.0.0
