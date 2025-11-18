#if MONGODB_HAS_SEARCHINDEXES
using MongoDB.Driver;

namespace MongoTypeRepository
{
    public partial class ThrottledMongoCollection<T>
    {
        // Forward typed SearchIndexes when building against a driver that exposes IMongoSearchIndexManager
        public IMongoSearchIndexManager SearchIndexes => _base.SearchIndexes;
    }
}
#endif