# Changelog

All notable changes to `sebkuw.Cqrs` are documented here. The format follows Keep a Changelog and Semantic Versioning.

## [Unreleased]

### Added

- Package-specific documentation, development instructions, changelog, and quality workflow.

### Changed

- Clarified handler namespaces, scoped resolution, non-public scanning, and registration of additional implemented interfaces.
- Renamed the package, assembly, project, and public namespaces to `sebkuw.Cqrs`; version 2.0.0 requires consumers to update package references and imports.
- Centralized dependency versions across the repository.
- Declared the package's MIT license in NuGet metadata.

## [1.0.0] - 2026-08-11

### Added

- Explicit assembly scanning and scoped DI registration for all sebkuw CQRS handler contracts.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/sebkuw.Cqrs-v1.0.0...HEAD
[1.0.0]: https://github.com/sebkuw/dotnet-building-blocks/releases/tag/sebkuw.Cqrs-v1.0.0
