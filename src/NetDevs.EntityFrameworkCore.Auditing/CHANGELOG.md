# Changelog

All notable changes to `NetDevs.EntityFrameworkCore.Auditing` are documented here. The format follows Keep a Changelog and Semantic Versioning.

## [Unreleased]

### Added

- Package-specific documentation, development instructions, changelog, and quality workflow.

### Changed

- Centralized EF Core and dependency-injection package versions.
- Declared the package's MIT license in NuGet metadata.

## [1.0.0] - 2026-08-11

### Added

- SaveChanges interceptor for creation and update audit metadata.
- Replaceable current-user and UTC-time providers with DI registration.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/NetDevs.EntityFrameworkCore.Auditing-v1.0.0...HEAD
[1.0.0]: https://github.com/sebkuw/dotnet-building-blocks/releases/tag/NetDevs.EntityFrameworkCore.Auditing-v1.0.0
