using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoTypeRepository;
using MongoTypeRepository.Contracts;

namespace MongoTypeRepository.Tests
{
    /// <summary>A minimal document type used across the test specs.</summary>
    public class TestItem : IMongoItem
    {
        public ObjectId Id { get; set; }
        public string? Name { get; set; }
    }

    /// <summary>
    /// Concrete repository over the injected (mocked) collection, using the
    /// internal test-seam constructor on <see cref="TypeRepositoryBase{Tdb}"/>.
    /// Also exposes the internal static <c>PreparePagingFilter</c> for the
    /// paging-filter specs.
    /// </summary>
    public class TestRepository : TypeRepositoryBase<TestItem>
    {
        public TestRepository(IMongoCollection<TestItem> collection) : base(collection)
        {
        }

        public static FilterDefinition<TestItem> BuildPagingFilter(RepositoryPaging paging)
        {
            return PreparePagingFilter(null, paging);
        }
    }

    /// <summary>An <see cref="IAsyncCursor{T}"/> stub yielding no items.</summary>
    public static class TestCursor
    {
        public static IAsyncCursor<T> Empty<T>() => new EmptyCursor<T>();

        private sealed class EmptyCursor<T> : IAsyncCursor<T>
        {
            public IEnumerable<T> Current => new List<T>();

            public bool MoveNext(CancellationToken cancellationToken = default) => false;

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(false);

            public void Dispose()
            {
            }
        }
    }
}
