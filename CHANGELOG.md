# Changelog

All notable repository-wide changes are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html) per package.

## [Unreleased]

### Added

- Hierarchical `AGENTS.md` guidance for the repository, tests, and every production library.
- Repository-local development skills for cross-cutting work and all six NuGet packages.
- Per-package README and changelog files included in NuGet packages.
- A documented compatibility contract with `angular-shared-components`.
- CI quality gates for formatting, Release build, tests, coverage, licenses, vulnerabilities, packing, and package contents.
- The missing `NetDevs.Cqrs.Abstractions.Tests` project.

### Changed

- Standardized the repository on the .NET 10 SDK feature band and C# 14.
- Centralized NuGet package versions and enabled deterministic locked restore.
- Scoped dependency restore to the public `nuget.org` source with explicit source mapping.
- Enabled nullable analysis, recommended .NET analyzers, warnings as errors, Source Link, and deterministic builds.
- Declared the repository's MIT license in every NuGet package and added package-content validation for it.
- Localized all repository and package `AGENTS.md` instructions, development skills, and skill interface metadata into Polish.

[Unreleased]: https://github.com/sebkuw/dotnet-building-blocks/compare/HEAD...HEAD
