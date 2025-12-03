# Rammi.MongoTypeRepository

A strongly typed repository library for MongoDB and Azure Cosmos DB, providing full async support with built-in connection throttling to prevent connection pool exhaustion.

## Features

- ✅ **Strongly Typed**: Provides strong types and repository access for MongoDB and Azure Cosmos DB
- ✅ **Full Async Support**: All operations support async/await patterns
- ✅ **Connection Throttling**: Built-in throttling prevents `MongoWaitQueueFullException` by limiting concurrent operations
- ✅ **Type Safety**: Strong typing eliminates runtime errors from weak-typed MongoDB access
- ✅ **Easy to Use**: Simple repository pattern implementation

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Rammi.MongoTypeRepository
```

Or via Package Manager Console:

```powershell
Install-Package Rammi.MongoTypeRepository
```

## Quick Start

```csharp
// Define your data model
public class Book : IMongoItem
{
    public ObjectId Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
}

// Create your repository
public class BooksRepository : TypeRepositoryBase<Book>
{
    public BooksRepository(IMongoDatabase database) : base(database, "books") { }
}

// Use it
var repository = new BooksRepository(mongoDatabase);
var books = await repository.GetAllAsync();
```

## Why Use This Library?

MongoDB and Azure Cosmos DB are weakly typed, which can lead to runtime errors. This library provides:

- **Type Safety**: Catch errors at compile time instead of runtime
- **Connection Management**: Prevents connection pool exhaustion with automatic throttling
- **Clean Code**: Repository pattern keeps your code organized and testable

## Documentation

For complete documentation, examples, and source code, visit:
- **GitHub**: https://github.com/rammicz/MongoRepo
- **Author**: [Jiri Hernik](http://rammi.cz)

## License

This project is licensed under the MIT License.

## Author

**Jiri Hernik**

- Website: [rammi.cz](http://rammi.cz)
- GitHub: [@rammicz](https://github.com/rammicz)





