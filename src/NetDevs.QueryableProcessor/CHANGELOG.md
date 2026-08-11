# Changelog

All notable changes to `NetDevs.QueryableProcessor` are documented here. The format follows Keep a Changelog and Semantic Versioning.

## [Unreleased]

### Added

- Contract tests and documentation for `@netdevs/shared-ui-list` requests and responses.
- Package-specific development instructions and quality workflow.

### Changed

- Accept Angular sort values such as `createdAt desc` while preserving `desc_CreatedAt` compatibility.
- Resolve sort property paths case-insensitively so camelCase frontend fields match .NET properties.
- Resolve filter property paths case-insensitively so Angular camelCase fields match .NET properties.
- Pin query request and pagination response JSON names to the PascalCase contract expected by the Angular adapter.
- Centralize EF Core package versions.
- Declare the package's MIT license in NuGet metadata.

### Fixed

- Reject malformed GUID filter values instead of silently converting them to `Guid.Empty`.

## [1.0.0] - 2026-08-11

### Added

- Dynamic filtering, sorting, pagination, projection, and pagination metadata for `IQueryable<T>`.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/NetDevs.QueryableProcessor-v1.0.0...HEAD
[1.0.0]: https://github.com/sebkuw/dotnet-building-blocks/releases/tag/NetDevs.QueryableProcessor-v1.0.0
