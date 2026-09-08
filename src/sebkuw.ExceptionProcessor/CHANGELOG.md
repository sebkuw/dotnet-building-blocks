# Changelog

All notable changes to `sebkuw.ExceptionProcessor` are documented here. The format follows Keep a Changelog and Semantic Versioning.

## [Unreleased]

### Added

- Package-specific documentation, development instructions, changelog, and Angular-facing contract guidance.
- A full-pipeline correlation ID regression test.

### Changed

- Renamed the package, assembly, project, and public namespaces to `sebkuw.ExceptionProcessor`; version 2.0.0 requires consumers to update package references and imports.
- Replace the obsolete ASP.NET Core 2.3.9 package dependency with the .NET 10 shared framework reference.
- Declare the package's MIT license in NuGet metadata.

### Fixed

- Preserve a client-provided correlation ID in the error response header and JSON body when both middleware components are used.
- Avoid duplicate response-header insertion by assigning the correlation header deterministically.

## [1.0.0] - 2026-08-11

### Added

- Exception mapping, built-in application exception types, structured JSON middleware, correlation middleware, and NLog adapter.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/sebkuw.ExceptionProcessor-v1.0.0...HEAD
[1.0.0]: https://github.com/sebkuw/dotnet-building-blocks/releases/tag/sebkuw.ExceptionProcessor-v1.0.0
