# Changelog

All notable changes to `NetDevs.Domain.Abstractions` are documented here. The format follows Keep a Changelog and Semantic Versioning.

## [Unreleased]

### Added

- Package-specific documentation, development instructions, changelog, and quality workflow.
- `IHardDeleteProtected`, `IDeactivatable`, and `IAppendOnlyEntity` lifecycle contracts.
- `CreatedAuditableEntity<TId>` for creation-only audit metadata.
- `DeactivatableAuditableEntity<TId>` with explicit, idempotent deactivation and reactivation operations.

### Changed

- Declared the package's MIT license in NuGet metadata.
- `AuditableEntity<TId>` now inherits creation metadata from `CreatedAuditableEntity<TId>` without changing its existing public properties.
- Advanced the package version to `1.1.0` for the additive public API.

## [1.0.0] - 2026-08-11

### Added

- Generic and GUID entity contracts and base types.
- Creation and update auditing contracts and auditable base entities.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/NetDevs.Domain.Abstractions-v1.0.0...HEAD
[1.0.0]: https://github.com/sebkuw/dotnet-building-blocks/releases/tag/NetDevs.Domain.Abstractions-v1.0.0
