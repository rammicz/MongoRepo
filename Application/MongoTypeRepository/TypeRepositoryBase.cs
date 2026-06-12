using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoTypeRepository.Contracts;

namespace MongoTypeRepository
{
    internal static class MongoClientCache
    {
        private static readonly ConcurrentDictionary<string, MongoClient> Clients
            = new ConcurrentDictionary<string, MongoClient>();

        internal static MongoClient GetOrCreate(string key, Func<MongoClient> factory)
        {
            return Clients.GetOrAdd(key, _ => factory());
        }
    }

    public abstract class TypeRepositoryBase<Tdb> : ITypeRepositoryBase<Tdb> where Tdb : IMongoItem
    {
        /// <summary>
        /// Creates a TypeRepository and connects do DB
        /// </summary>
        /// <param name="url"></param>
        /// <param name="collectionName"></param>
        /// <param name="concurentTaskLimit">How many tasks can call DB concurrently
        ///  0 = half of the current connection pool
        /// -1 = no throttling at all.
        ///  n = number of concurent calls
        /// </param>
        public TypeRepositoryBase(MongoUrl url, string collectionName, int concurentTaskLimit = 0)
        {
            MongoClient = MongoClientCache.GetOrCreate(url.ToString(), () => new MongoClient(url));
            SetUp(url.DatabaseName, collectionName, concurentTaskLimit);
        }

        /// <summary>
        /// Creates a TypeRepository and connects do DB
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="collectionName"></param>
        /// <param name="concurentTaskLimit">How many tasks can call DB concurrently
        ///  0 = half of the current connection pool
        /// -1 = no throttling at all.
        ///  n = number of concurent calls
        /// </param>
        public TypeRepositoryBase(string connectionString, string collectionName, int concurentTaskLimit = 0)
        {
            MongoUrl url = MongoUrl.Create(connectionString);
            MongoClient = MongoClientCache.GetOrCreate(connectionString, () => new MongoClient(connectionString));
            SetUp(url.DatabaseName, collectionName, concurentTaskLimit);
        }

        /// <summary>
        /// Creates a TypeRepository and connects do local DB
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="collectionName"></param>
        /// <param name="isLocal"></param>
        /// <param name="concurentTaskLimit">How many tasks can call DB concurrently
        ///  0 = half of the current connection pool
        /// -1 = no throttling at all.
        ///  n = number of concurent calls
        /// </param>
        public TypeRepositoryBase(string databaseName, string collectionName, bool isLocal, int concurentTaskLimit = 0)
        {
            if (!isLocal)
            {
                throw new Exception("This method can run only on local DB");
            }

            MongoClient = MongoClientCache.GetOrCreate("__local__", () => new MongoClient());
            SetUp(databaseName, collectionName, concurentTaskLimit);
        }

        /// <summary>
        /// Test seam: builds a repository directly over a supplied collection
        /// (e.g. a mocked <see cref="IMongoCollection{Tdb}"/>) with no live
        /// connection. Used by the unit test project; see InternalsVisibleTo.
        /// Note: <see cref="MongoClient"/> intentionally stays null on this path;
        /// the seam supports only Collection-routed operations.
        /// </summary>
        internal TypeRepositoryBase(IMongoCollection<Tdb> collection)
        {
            Collection = collection;
        }

        public IMongoCollection<Tdb> Collection { get; private set; }
        public MongoClient MongoClient { get; }
        public IQueryable<Tdb> CollectionQuery => Collection.AsQueryable();

        public Tdb GetById(string id)
        {
            return GetById(ObjectId.Parse(id));
        }

        public Tdb GetById(ObjectId id)
        {
            return Collection.Find(IdFilter(id)).SingleOrDefault();
        }

        /// <summary>
        ///     Replaces documents in DB, based on _id.
        ///     Issues a single BulkWrite (one ReplaceOneModel per item, upsert off);
        ///     an empty input is a no-op (the driver rejects an empty request list).
        /// </summary>
        /// <param name="objectsToSave"></param>
        public void Update(IEnumerable<Tdb> objectsToSave)
        {
            BulkReplace(objectsToSave, isUpsert: false, generateMissingIds: false);
        }

        /// <summary>
        ///     Replaces document in DB, based on _id
        /// </summary>
        /// <param name="objectToSave"></param>
        public ReplaceOneResult Update(Tdb objectToSave)
        {
            var updateOptions = new ReplaceOptions { IsUpsert = false }; // update or insert / upsert
            return Collection.ReplaceOne(IdFilter(objectToSave.Id), objectToSave, updateOptions);
        }

        public void Insert(Tdb item)
        {
            Collection.InsertOne(item);
        }

        public void Insert(IEnumerable<Tdb> items)
        {
            Collection.InsertMany(items);
        }

        public void Delete(IMongoItem objectToDelete)
        {
            Delete(objectToDelete.Id);
        }

        public void Delete(string id)
        {
            // Parse to ObjectId first - the _id field is stored as ObjectId, so
            // a raw string filter would never match (silent no-op).
            Delete(ObjectId.Parse(id));
        }

        public void Delete(ObjectId id)
        {
            Collection.DeleteOne(IdFilter(id));
        }

        public DeleteResult DeleteAll()
        {
            return Collection.DeleteMany(FilterDefinition<Tdb>.Empty);
        }

        public List<Tdb> GetPagedResults(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging)
        {
            SetPaging(paging);

            FilterDefinition<Tdb> totalFilter = PreparePagingFilter(primaryFilters, paging);

            IFindFluent<Tdb, Tdb> filtered = Collection.Find(totalFilter);

            paging.TotalItems = filtered.CountDocuments();

            var sort = SetSorting(paging);

            filtered = filtered.Sort(sort);

            // runs over cursor in MongoDb
            List<Tdb> items = filtered.Skip(GetSkip(paging)).Limit(paging.PageSize).ToList();

            return items;
        }

        /// <summary>
        ///     Replace or insert document in DB
        /// </summary>
        /// <param name="objectToSave"></param>
        public void Save(Tdb objectToSave)
        {
            EnsureId(objectToSave);
            var updateOptions = new ReplaceOptions { IsUpsert = true }; // update or insert / upsert
            Collection.ReplaceOne(IdFilter(objectToSave.Id), objectToSave, updateOptions);
        }

        public async Task<Tdb> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await GetByIdAsync(ObjectId.Parse(id), cancellationToken);
        }

        public async Task<Tdb> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (await Collection.FindAsync(IdFilter(id), cancellationToken: cancellationToken)).SingleOrDefault();
        }

        public List<Tdb> Find(FilterDefinition<Tdb> filter)
        {
            return Find(filter, null, null);
        }

        public List<Tdb> Find(FilterDefinition<Tdb> filter, SortDefinition<Tdb> sort)
        {
            return Find(filter, sort, null);
        }

        public List<Tdb> Find(FilterDefinition<Tdb> filter, SortDefinition<Tdb> sort, int? limit)
        {
            var query = Collection.Find(filter ?? FilterDefinition<Tdb>.Empty);

            if (sort != null)
            {
                query = query.Sort(sort);
            }

            if (limit.HasValue)
            {
                query = query.Limit(limit.Value);
            }

            return query.ToList();
        }

        public Task<List<Tdb>> FindAsync(FilterDefinition<Tdb> filter, CancellationToken cancellationToken = default)
        {
            return FindAsync(filter, null, null, cancellationToken);
        }

        public Task<List<Tdb>> FindAsync(FilterDefinition<Tdb> filter, SortDefinition<Tdb> sort, CancellationToken cancellationToken = default)
        {
            return FindAsync(filter, sort, null, cancellationToken);
        }

        public async Task<List<Tdb>> FindAsync(FilterDefinition<Tdb> filter, SortDefinition<Tdb> sort, int? limit, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var options = new FindOptions<Tdb, Tdb>();

            if (sort != null)
            {
                options.Sort = sort;
            }

            if (limit.HasValue)
            {
                options.Limit = limit.Value;
            }

            var cursor = await Collection.FindAsync(filter ?? FilterDefinition<Tdb>.Empty, options, cancellationToken);
            return await cursor.ToListAsync(cancellationToken);
        }

        /// <summary>
        ///     Replace or insert documents in DB, based on _id.
        ///     Issues a single BulkWrite (one ReplaceOneModel per item, upsert on);
        ///     items with an empty Id get a freshly generated one first.
        ///     An empty input is a no-op (the driver rejects an empty request list).
        /// </summary>
        /// <param name="objectsToSave"></param>
        public void Save(IEnumerable<Tdb> objectsToSave)
        {
            BulkReplace(objectsToSave, isUpsert: true, generateMissingIds: true);
        }

        /// <summary>
        ///     Replace or insert documents in DB, based on _id.
        ///     Issues a single BulkWriteAsync (one ReplaceOneModel per item, upsert on);
        ///     items with an empty Id get a freshly generated one first.
        ///     An empty input is a no-op (the driver rejects an empty request list).
        /// </summary>
        public Task SaveAsync(IEnumerable<Tdb> objectsToSave, CancellationToken cancellationToken = default)
        {
            return BulkReplaceAsync(objectsToSave, isUpsert: true, generateMissingIds: true, cancellationToken);
        }

        /// <summary>
        ///     Replace or insert document in DB
        /// </summary>
        /// <param name="objectToSave"></param>
        public async Task SaveAsync(Tdb objectToSave, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureId(objectToSave);
            var updateOptions = new ReplaceOptions { IsUpsert = true }; // update or insert / upsert
            await Collection.ReplaceOneAsync(IdFilter(objectToSave.Id), objectToSave, updateOptions, cancellationToken);
        }

        /// <summary>
        ///     Replaces documents in DB, based on _id.
        ///     Issues a single BulkWriteAsync (one ReplaceOneModel per item, upsert off);
        ///     an empty input is a no-op (the driver rejects an empty request list).
        ///     Returns <see cref="Task"/> per the interface contract; the BulkWriteResult is discarded.
        /// </summary>
        /// <param name="objectsToSave"></param>
        public Task UpdateAsync(IEnumerable<Tdb> objectsToSave, CancellationToken cancellationToken = default)
        {
            return BulkReplaceAsync(objectsToSave, isUpsert: false, generateMissingIds: false, cancellationToken);
        }

        /// <summary>
        ///     Replaces document in DB, based on _id
        /// </summary>
        /// <param name="objectToSave"></param>
        public async Task<ReplaceOneResult> UpdateAsync(Tdb objectToSave, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var updateOptions = new ReplaceOptions { IsUpsert = false }; // update or insert / upsert
            return await Collection.ReplaceOneAsync(IdFilter(objectToSave.Id), objectToSave, updateOptions, cancellationToken);
        }

        private void BulkReplace(IEnumerable<Tdb> objectsToSave, bool isUpsert, bool generateMissingIds)
        {
            List<WriteModel<Tdb>> models = BuildReplaceModels(objectsToSave, isUpsert, generateMissingIds);
            if (models.Count > 0)
            {
                Collection.BulkWrite(models);
            }
        }

        private async Task BulkReplaceAsync(IEnumerable<Tdb> objectsToSave, bool isUpsert, bool generateMissingIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<WriteModel<Tdb>> models = BuildReplaceModels(objectsToSave, isUpsert, generateMissingIds);
            if (models.Count > 0)
            {
                await Collection.BulkWriteAsync(models, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        ///     Builds the ReplaceOneModel list shared by the batched Save/Update overloads.
        ///     For Save (<paramref name="generateMissingIds"/> = true) an empty Id is assigned a
        ///     freshly generated ObjectId before the model is built, mirroring single-item Save.
        /// </summary>
        private static List<WriteModel<Tdb>> BuildReplaceModels(IEnumerable<Tdb> objectsToSave, bool isUpsert, bool generateMissingIds)
        {
            var models = new List<WriteModel<Tdb>>(objectsToSave is ICollection<Tdb> c ? c.Count : 0);
            foreach (Tdb objectToSave in objectsToSave)
            {
                if (generateMissingIds)
                {
                    EnsureId(objectToSave);
                }

                models.Add(new ReplaceOneModel<Tdb>(IdFilter(objectToSave.Id), objectToSave) { IsUpsert = isUpsert });
            }

            return models;
        }

        /// <summary>Single construction point for the by-_id filter used across CRUD paths.</summary>
        private static FilterDefinition<Tdb> IdFilter(ObjectId id)
        {
            return new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", id));
        }

        /// <summary>Assigns a freshly generated ObjectId when the item's Id is empty (Save semantics).</summary>
        private static void EnsureId(Tdb obj)
        {
            if (obj.Id == ObjectId.Empty)
            {
                obj.Id = ObjectId.GenerateNewId();
            }
        }

        public async Task InsertAsync(Tdb item, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Collection.InsertOneAsync(item, cancellationToken: cancellationToken);
        }

        public async Task InsertAsync(IEnumerable<Tdb> items, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Collection.InsertManyAsync(items, cancellationToken: cancellationToken);
        }

        public async Task<DeleteResult> DeleteAsync(IMongoItem objectToDelete, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(objectToDelete.Id, cancellationToken);
        }

        public async Task<DeleteResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            // Parse to ObjectId first - the _id field is stored as ObjectId, so
            // a raw string filter would never match (silent no-op).
            return await DeleteAsync(ObjectId.Parse(id), cancellationToken);
        }

        public async Task<DeleteResult> DeleteAsync(ObjectId id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Collection.DeleteOneAsync(IdFilter(id), cancellationToken);
        }

        public async Task<DeleteResult> DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Collection.DeleteManyAsync(FilterDefinition<Tdb>.Empty, cancellationToken);
        }

        public async Task<List<Tdb>> GetPagedResultsAsync(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SetPaging(paging);

            FilterDefinition<Tdb> totalFilter = PreparePagingFilter(primaryFilters, paging);
            var countTask = Collection.CountDocumentsAsync(totalFilter, cancellationToken: cancellationToken);

            var sorting = SetSorting(paging);

            // Server-side skip/limit: only the requested page is transferred and deserialized.
            var findOptions = new FindOptions<Tdb, Tdb>
            {
                Sort = sorting,
                Skip = GetSkip(paging),
                Limit = paging.PageSize,
            };

            var cursor = await Collection.FindAsync(totalFilter, findOptions, cancellationToken);
            var result = await cursor.ToListAsync(cancellationToken);

            paging.TotalItems = await countTask;
            return result;
        }

        private static void SetPaging(RepositoryPaging paging)
        {
            if (paging.CurrentPage < 1)
            {
                paging.CurrentPage = 1;
            }

            if (paging.PageSize < 1)
            {
                paging.PageSize = 1;
            }
        }

        private static int GetSkip(RepositoryPaging paging)
        {
            return (paging.CurrentPage - 1) * paging.PageSize;
        }

        private static SortDefinition<Tdb> SetSorting(RepositoryPaging paging)
        {
            if (!string.IsNullOrWhiteSpace(paging.OrderBy))
            {
                SortDefinition<Tdb> sort;
                if (paging.OrderDirection == Ordering.asc)
                {
                    sort = Builders<Tdb>.Sort.Ascending(paging.OrderBy);
                }
                else
                {
                    sort = Builders<Tdb>.Sort.Descending(paging.OrderBy);
                }

                return sort;
            }

            return null;
        }

        internal static FilterDefinition<Tdb> PreparePagingFilter(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging)
        {
            var fb = new FilterDefinitionBuilder<Tdb>();
            FilterDefinition<Tdb> totalFilter = primaryFilters ?? Builders<Tdb>.Filter.Empty; // or maybe  JsonFilterDefinition<Tdb>.Empty

            if (paging.Filtering != null)
            {
                foreach (Filtering filtering in paging.Filtering)
                {
                    FilterDefinition<Tdb> filter = null;
                    int valueInt = 0;
                    bool isInt = int.TryParse(filtering.Value, out valueInt);

                    switch (filtering.Operator)
                    {
                        case FilterOperator.Equals:
                            bool hodnotaBool;

                            if (bool.TryParse(filtering.Value, out hodnotaBool))
                            {
                                filter = Builders<Tdb>.Filter.Eq(filtering.By, hodnotaBool);

                                if (!hodnotaBool)
                                {
                                    filter |= Builders<Tdb>.Filter.Eq(filtering.By, BsonNull.Value);
                                }
                            }
                            else
                            {
                                filter = Builders<Tdb>.Filter.Eq(filtering.By, filtering.Value);

                                if (isInt)
                                {
                                    filter |= Builders<Tdb>.Filter.Eq(filtering.By, valueInt);
                                }
                            }

                            totalFilter &= filter;
                            break;
                        case FilterOperator.Contains:
                            filter = RegexFilter(filtering.By, filtering.Value);
                            totalFilter &= filter;
                            break;
                        case FilterOperator.StartsWith:
                            filter = RegexFilter(filtering.By, filtering.Value, prefix: "^");
                            totalFilter &= filter;
                            break;
                        case FilterOperator.EndsWidth:
                            filter = RegexFilter(filtering.By, filtering.Value, suffix: "$");
                            totalFilter &= filter;
                            break;
                        case FilterOperator.GreaterThan:

                            filter = Builders<Tdb>.Filter.Gt(filtering.By, valueInt);
                            totalFilter &= filter;
                            break;
                        case FilterOperator.GreaterThanOrEquals:
                            filter = Builders<Tdb>.Filter.Gte(filtering.By, valueInt);
                            totalFilter &= filter;
                            break;
                        case FilterOperator.LessThan:
                            filter = Builders<Tdb>.Filter.Lt(filtering.By, valueInt);
                            totalFilter &= filter;
                            break;
                        case FilterOperator.LessThanOrEquals:
                            filter = Builders<Tdb>.Filter.Lte(filtering.By, valueInt);
                            totalFilter &= filter;
                            break;
                        default:
                            throw new NotSupportedException("Unsupported filtering operator");
                    }
                }
            }

            return totalFilter;
        }

        /// <summary>
        /// Builds a case-insensitive regex filter from a user-supplied filter value.
        /// Single construction point for paging regex filters: the value always passes
        /// through <see cref="Regex.Escape(string)"/> (regex injection / ReDoS guard, #12)
        /// and a null value degrades to a match-anything pattern instead of throwing.
        /// </summary>
        private static FilterDefinition<Tdb> RegexFilter(string field, string value, string prefix = "", string suffix = "")
        {
            return Builders<Tdb>.Filter.Regex(field, new BsonRegularExpression(prefix + Regex.Escape(value ?? string.Empty) + suffix, "i"));
        }

        private void SetUp(string databaseName, string collectionName, int concurentTaskLimit = 0)
        {
            IMongoDatabase db = MongoClient.GetDatabase(databaseName);

            if (concurentTaskLimit < 1)
            {
                if (concurentTaskLimit == 0)
                {
                    // default is half of the poolsize
                    concurentTaskLimit = MongoClient.Settings.MaxConnectionPoolSize / 2;
                }
                else
                {
                    // No throttling if Limit is < 0
                    Collection = db.GetCollection<Tdb>(collectionName);
                    return;
                }
            }
            
            Collection = new ThrottledMongoCollection<Tdb>(db.GetCollection<Tdb>(collectionName), concurentTaskLimit);
        }
    }
}