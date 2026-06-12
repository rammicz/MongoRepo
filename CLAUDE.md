# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build entire solution
dotnet build MongoRepo.sln

# Build core library only
dotnet build Application/MongoTypeRepository/MongoTypeRepository.csproj

# Build example app only
dotnet build Application/MongoTypeRepositoryExample/MongoTypeRepositoryExample.csproj

# Run example app (requires local MongoDB or configured connection string)
dotnet run --project Application/MongoTypeRepositoryExample/MongoTypeRepositoryExample.csproj

# Run the unit tests (no live MongoDB required - collections are mocked)
dotnet test Application/MongoTypeRepository.Tests/MongoTypeRepository.Tests.csproj
```

Tests live in `Application/MongoTypeRepository.Tests` (xUnit + Moq, targets net10.0). They use mocked `IMongoCollection<T>` via the internal test-seam constructor on `TypeRepositoryBase<Tdb>` (`InternalsVisibleTo("MongoTypeRepository.Tests")`), so no live MongoDB is needed. CI runs `dotnet test` on every push/PR (`.github/workflows/build.yml`) and before packing in the publish workflow; a failing test fails the build.

## NuGet Package

Package ID: `Rammi.MongoTypeRepository`. Spec file: `Application/MongoTypeRepository/Rammi.MongoTypeRepository.nuspec`. Multi-targets: net48, netstandard2.1, net8.0, net9.0, net10.0. Language version is C# 9 with nullable enabled.

## Architecture

This is a strongly-typed generic repository library for MongoDB (and Azure Cosmos DB). The core library is `MongoTypeRepository`; `MongoTypeRepositoryExample` is a demo console app.

### Core abstractions

- **`IMongoItem`** — Base interface all document types must implement (provides `ObjectId Id`).
- **`ITypeRepositoryBase<Tdb>`** — Repository contract: async/sync CRUD, Find with `FilterDefinition`/`SortDefinition`, paged queries with filtering/sorting.
- **`TypeRepositoryBase<Tdb>`** — Abstract base implementation. Accepts connection string, `MongoUrl`, or local DB reference. Consumers subclass this per entity type.

### Connection throttling

`TypeRepositoryBase` wraps the underlying `IMongoCollection<T>` in a **`ThrottledMongoCollection<T>`** (decorator). All async operations pass through a **`ThrottlingSemaphore`** (extends `SemaphoreSlim`) to prevent `MongoWaitQueueFullException`. Sync operations are *not* throttled. The concurrency limit is configured via the `concurrentTaskLimit` constructor parameter (`0` = half pool size, `-1` = unlimited, `n` = explicit limit).

### Wrapping non-IMongoItem types

`AnyTypeWrapper<T>` wraps arbitrary objects so they can be stored as MongoDB documents. `WrappingHelper.Wrap` converts collections. See the `AlienShipsRepository` example.

### Filtering & Paging

`RepositoryPaging` carries page/size/ordering plus a list of `Filtering` objects (field, operator, value). `FilterOperator` enum: Equals, Contains, StartsWith, EndsWidth, GreaterThan, LessThan, GreaterThanOrEquals, LessThanOrEquals.
