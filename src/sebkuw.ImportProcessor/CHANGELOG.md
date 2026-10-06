# Changelog

All notable changes to `sebkuw.ImportProcessor` are documented here. The format follows Keep a Changelog and Semantic Versioning.

## [Unreleased]

### Added

- Bounded concurrent mapping and validation controlled by `MaxDegreeOfParallelism` and `BufferCapacity` while preserving input order.
- Incremental writer batches controlled by `BatchSize` and bounded result retention controlled by `MaxRetainedRows`.
- Complete processed, valid, and invalid row counts even when the returned row report is intentionally truncated.

### Changed

- Added a complete preview example and documented record memory limits, mapping requirements, idempotency behavior, and safe report handling.
- Advanced the package version to 2.1.0 for the additive large-file import API.
- Organized source files by abstractions, configuration, internals, mapping, models, and processing without changing public namespaces or behavior.

## [2.0.0] - 2026-09-10

### Added

- Initial streaming CSV import pipeline with generic mapping, culture-aware conversion, row validation, preview, batch and error reporting, application-owned writing, and idempotency extension points.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/sebkuw.ImportProcessor-v2.0.0...HEAD
[2.0.0]: https://github.com/sebkuw/dotnet-building-blocks/releases/tag/sebkuw.ImportProcessor-v2.0.0
