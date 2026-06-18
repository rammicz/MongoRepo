# MongoRepo

[![NuGet](https://img.shields.io/nuget/v/Rammi.MongoTypeRepository.svg)](https://www.nuget.org/packages/Rammi.MongoTypeRepository/)
[![Build and Test](https://github.com/rammicz/MongoRepo/actions/workflows/build.yml/badge.svg)](https://github.com/rammicz/MongoRepo/actions/workflows/build.yml)

A strongly-typed generic repository for **MongoDB** and **Azure Cosmos DB** (both of which are otherwise weakly typed). You define a model and a repository class; you get typed async/sync CRUD, LINQ querying, server-side paging/filtering, and built-in connection-pool throttling.

```csharp
public class Book : IMongoItem
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
}

public class BooksRepository : TypeRepositoryBase<Book>
{
    public BooksRepository(string connectionString)
        : base(connectionString, "books") { }
}

var repo = new BooksRepository("mongodb://localhost:27017/mydb");
await repo.SaveAsync(new Book { Name = "Dune", Author = "Herbert" });
var book = await repo.GetByIdAsync(id, cancellationToken);
```

## Features

- **Strong typing** over `IMongoCollection<T>` — no `BsonDocument` juggling in your code.
- **Full async support** — every operation has an `…Async` overload taking an optional trailing `CancellationToken`.
- **Connection-pool throttling** — async operations pass through a semaphore so you don't hit
  `MongoDB.Driver.MongoWaitQueueFullException: The wait queue for acquiring a connection to server … is full.`
- **Server-side paging & filtering** — `GetPagedResults`/`GetPagedResultsAsync` page with `Skip`/`Limit` and accept a typed filter list; text filters are regex-escaped (no injection/ReDoS).
- **Batched writes** — `Save`/`Update` of an `IEnumerable<T>` issues a single `BulkWrite`.
- **Wrap non-typed objects** — store types that don't implement `IMongoItem` via `AnyTypeWrapper<T>`.
- **LINQ access** — `CollectionQuery` exposes `IQueryable<T>`; `Collection` exposes the raw driver collection for advanced queries.

## Install

```bash
dotnet add package Rammi.MongoTypeRepository
```

Targets `net10.0`, `net8.0`, `netstandard2.1`, and `net472`. Depends on `MongoDB.Driver` 3.9.0.

## Getting started

### 1. Define a model

Every document implements `IMongoItem`, which requires an `ObjectId Id`:

```csharp
using MongoDB.Bson;
using MongoTypeRepository;

public class Book : IMongoItem
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public DateTime DatePublished { get; set; }
}
```

### 2. Define a repository

Subclass `TypeRepositoryBase<T>` and pass the connection details to the base constructor:

```csharp
using MongoTypeRepository;

public class BooksRepository : TypeRepositoryBase<Book>
{
    // Connection string + collection name
    public BooksRepository(string connectionString)
        : base(connectionString, "books") { }

    // You can add your own typed helpers on top of the base API:
    public Task<List<Book>> ByAuthorAsync(string author, CancellationToken ct = default)
        => FindAsync(Builders<Book>.Filter.Eq(b => b.Author, author), ct);
}
```

There are three base constructors:

```csharp
TypeRepositoryBase(string connectionString, string collectionName, int concurrentTaskLimit = 0)
TypeRepositoryBase(MongoUrl url,           string collectionName, int concurrentTaskLimit = 0)
TypeRepositoryBase(string databaseName,    string collectionName, bool isLocal, int concurrentTaskLimit = 0)
```

### 3. Use it

```csharp
var repo = new BooksRepository("mongodb://localhost:27017/library");

var book = new Book { Name = "Dune", Author = "Frank Herbert" };
await repo.SaveAsync(book);                 // insert or replace by Id
await repo.SaveAsync(manyBooks);            // single BulkWrite for the batch

var byId   = await repo.GetByIdAsync(book.Id);
var byAuth = await repo.FindAsync(Builders<Book>.Filter.Eq(b => b.Author, "Frank Herbert"));

book.Name = "Dune (revised)";
await repo.UpdateAsync(book);

await repo.DeleteAsync(book.Id);
```

Synchronous equivalents (`Save`, `GetById`, `Find`, `Update`, `Delete`, …) exist for non-async call sites. Sync operations are **not** throttled.

## Throttling

`TypeRepositoryBase` routes every async operation through a `ThrottlingSemaphore` to cap concurrent commands against the MongoDB connection pool. Control it with the `concurrentTaskLimit` constructor argument:

| Value | Behaviour |
|-------|-----------|
| `0` (default) | Half of the configured connection-pool size |
| `-1` | Unlimited (no throttling) |
| `n` | Explicit limit of `n` concurrent operations |

## Paging & filtering

`GetPagedResultsAsync` combines a typed `FilterDefinition<T>` with a `RepositoryPaging` describing the page, ordering, and a list of ad-hoc UI filters. Paging is applied server-side (`Skip`/`Limit`), and string filter values are regex-escaped.

```csharp
using MongoTypeRepository.Contracts;

var paging = new RepositoryPaging
{
    CurrentPage = 1,
    PageSize = 20,
    OrderBy = nameof(Book.Name),
    OrderDirection = Ordering.asc,
    Filtering = new List<Filtering>
    {
        new Filtering { By = nameof(Book.Author), Operator = FilterOperator.Contains, Value = "Herbert" }
    }
};

var page = await repo.GetPagedResultsAsync(Builders<Book>.Filter.Empty, paging);
// paging.TotalItems is populated with the unpaged match count.
```

`FilterOperator` values: `Equals`, `Contains`, `StartsWith`, `EndsWidth` *(note: spelled with a "d")*, `GreaterThan`, `LessThan`, `GreaterThanOrEquals`, `LessThanOrEquals`.

## Storing types that aren't `IMongoItem`

Wrap arbitrary objects with `AnyTypeWrapper<T>` so they get an `ObjectId` and can be persisted:

```csharp
using MongoTypeRepository.Wrapper;

public class WrappedAlienShip : AnyTypeWrapper<AlienShip>
{
    public WrappedAlienShip(AlienShip ship) : base(ship) => Id = ObjectId.GenerateNewId();
}

public class AlienShipsRepository : TypeRepositoryBase<WrappedAlienShip>
{
    public AlienShipsRepository(string conn) : base(conn, "alienShips") { }
}
```

The wrapped object lives under `Item`, so you filter into it by path (e.g. `Builders<WrappedAlienShip>.Filter.Eq("Item.Crew.Name", "Dracula")`). `WrappingHelper.Wrap<T, TDb>(items)` converts a collection in one call. See `Application/MongoTypeRepositoryExample` for the full alien-ship example.

## Building from source

```bash
dotnet build MongoRepo.sln
dotnet test  Application/MongoTypeRepository.Tests/MongoTypeRepository.Tests.csproj   # unit tests, no live MongoDB needed
dotnet run --project Application/MongoTypeRepositoryExample                            # demo app, needs a MongoDB connection
```

The test suite mocks `IMongoCollection<T>`, so no running MongoDB is required; CI runs it on every push and PR.

## License

[Apache License 2.0](LICENSE) © Jiri Hernik
