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

    /// <summary>An <see cref="IAsyncCursor{T}"/> stub returning a fixed set (default: none).</summary>
    public static class TestCursor
    {
        public static IAsyncCursor<T> Empty<T>() => new StubCursor<T>(new List<T>());

        public static IAsyncCursor<T> Of<T>(IEnumerable<T> items) => new StubCursor<T>(new List<T>(items));

        private sealed class StubCursor<T> : IAsyncCursor<T>
        {
            private readonly List<T> _items;
            private bool _moved;

            public StubCursor(List<T> items) => _items = items;

            public IEnumerable<T> Current => _moved ? _items : new List<T>();

            public bool MoveNext(CancellationToken cancellationToken = default)
            {
                if (_moved)
                {
                    return false;
                }

                _moved = true;
                return _items.Count > 0;
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(MoveNext(cancellationToken));

            public void Dispose()
            {
            }
        }
    }
}
