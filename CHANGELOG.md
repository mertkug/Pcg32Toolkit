# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog.

## [1.0.0] - 2026-02-22

### Added
- PCG32 implementation with deterministic seeding via `seed` and `stream`.
- OS entropy factory method: `PCG32.CreateFromOsEntropy()`.
- Random generation APIs: `NextUInt`, `NextBounded`, `NextInt`, `NextSingle`, `NextDouble`.
- Comprehensive xUnit test suite for deterministic behavior and edge cases.
- Mutation testing setup using Stryker.

### Changed
- Refactored bounded generation into `NextBoundedCore` for deterministic edge-case testing.
- Updated package metadata for NuGet publishing.

### Quality
- Mutation score improved to 89.47%.
