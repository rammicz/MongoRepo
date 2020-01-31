using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoTypeRepository.Contracts;

namespace MongoTypeRepository
{
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
            MongoClient = new MongoClient(url);
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
            MongoClient = new MongoClient(connectionString);
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

            MongoClient = new MongoClient();
            SetUp(databaseName, collectionName, concurentTaskLimit);
        }

        public IMongoCollection<Tdb> Collection { get; private set; }
        public MongoClient MongoClient { get; }
        public IMongoQueryable<Tdb> CollectionQuery => Collection.AsQueryable();

        public Tdb GetById(string id)
        {
            return GetById(ObjectId.Parse(id));
        }

        public Tdb GetById(ObjectId id)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", id));
            return Collection.Find(filter).SingleOrDefault();
        }

        /// <summary>
        ///     Replaces documents in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        public void Update(IEnumerable<Tdb> objectsToSave)
        {
            foreach (Tdb obj in objectsToSave)
            {
                Update(obj);
            }
        }

        /// <summary>
        ///     Replaces document in DB, based on _id
        /// </summary>
        /// <param name="objectToSave"></param>
        public ReplaceOneResult Update(Tdb objectToSave)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToSave.Id));
            var updateOptions = new ReplaceOptions { IsUpsert = false }; // update or insert / upsert
            return Collection.ReplaceOne(filter, objectToSave, updateOptions);
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
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", id));
            Collection.DeleteOne(filter);
        }

        public void Delete(ObjectId id)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", id));
            Collection.DeleteOne(filter);
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
            List<Tdb> items = filtered.Skip((paging.CurrentPage - 1) * paging.PageSize).Limit(paging.PageSize).ToList();

            return items;
        }

        /// <summary>
        ///     Replace or insert document in DB
        /// </summary>
        /// <param name="objectToSave"></param>
        public void Save(Tdb objectToSave)
        {
            if (objectToSave.Id == ObjectId.Empty)
            {
                objectToSave.Id = ObjectId.GenerateNewId(DateTime.Now);
            }

            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToSave.Id));
            var updateOptions = new ReplaceOptions { IsUpsert = true }; // update or insert / upsert
            Collection.ReplaceOne(filter, objectToSave, updateOptions);
        }

        public async Task<Tdb> GetByIdAsync(string id)
        {
            return await GetByIdAsync(ObjectId.Parse(id));
        }

        public async Task<Tdb> GetByIdAsync(ObjectId id)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", id));
            return (await Collection.FindAsync(filter)).SingleOrDefault();
        }

        /// <summary>
        ///     Replace or insert documents in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        public void Save(IEnumerable<Tdb> objectsToSave)
        {
            foreach (Tdb objectToSave in objectsToSave)
            {
                Save(objectToSave);
            }
        }

        /// <summary>
        ///     Replace or insert documents in DB, based on _id
        /// </summary>
        public async Task SaveAsync(IEnumerable<Tdb> objectsToSave)
        {
            var tasks = objectsToSave.Select(SaveAsync);
            await Task.WhenAll(tasks);
        }

        /// <summary>
        ///     Replace or insert document in DB
        /// </summary>
        /// <param name="objectToSave"></param>
        public async Task SaveAsync(Tdb objectToSave)
        {
            if (objectToSave.Id == ObjectId.Empty)
            {
                objectToSave.Id = ObjectId.GenerateNewId(DateTime.Now);
            }

            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToSave.Id));
            var updateOptions = new ReplaceOptions { IsUpsert = true }; // update or insert / upsert
            await Collection.ReplaceOneAsync(filter, objectToSave, updateOptions);
        }

        /// <summary>
        ///     Replaces documents in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        public async Task UpdateAsync(IEnumerable<Tdb> objectsToSave)
        {
            var tasks = objectsToSave.Select(UpdateAsync);
            await Task.WhenAll(tasks);
        }

        /// <summary>
        ///     Replaces document in DB, based on _id
        /// </summary>
        /// <param name="objectToSave"></param>
        public async Task<ReplaceOneResult> UpdateAsync(Tdb objectToSave)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToSave.Id));
            var updateOptions = new ReplaceOptions { IsUpsert = false }; // update or insert / upsert
            return await Collection.ReplaceOneAsync(filter, objectToSave, updateOptions);
        }

        public async void InsertAsync(Tdb item)
        {
            await Collection.InsertOneAsync(item);
        }

        public async Task InsertAsync(IEnumerable<Tdb> items)
        {
            await Collection.InsertManyAsync(items);
        }

        public async Task<DeleteResult> DeleteAsync(IMongoItem objectToDelete)
        {
            return await DeleteAsync(objectToDelete.Id);
        }

        public async Task<DeleteResult> DeleteAsync(string id)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", id));
            return await Collection.DeleteOneAsync(filter);
        }

        public async Task<DeleteResult> DeleteAsync(ObjectId id)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", id));
            return await Collection.DeleteOneAsync(filter);
        }

        public async Task<DeleteResult> DeleteAllAsync()
        {
            return await Collection.DeleteManyAsync(FilterDefinition<Tdb>.Empty);
        }

        public async Task<List<Tdb>> GetPagedResultsAsync(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging)
        {
            SetPaging(paging);

            FilterDefinition<Tdb> totalFilter = PreparePagingFilter(primaryFilters, paging);
            var countTask = Collection.CountDocumentsAsync(totalFilter);

            var sorting = SetSorting(paging);

            var findOptions = new FindOptions<Tdb, Tdb> { Sort = sorting };
            var collection = await Collection.FindAsync(totalFilter, findOptions);
            var skip = (paging.CurrentPage - 1) * paging.PageSize;
            var result = collection.ToEnumerable().Skip(skip).Take(paging.PageSize).ToList();

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

        private static FilterDefinition<Tdb> PreparePagingFilter(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging)
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
                            filter = Builders<Tdb>.Filter.Regex(filtering.By, new BsonRegularExpression(string.Format(".*{0}.*", filtering.Value), "i"));
                            totalFilter &= filter;
                            break;
                        case FilterOperator.StartsWith:
                            filter = Builders<Tdb>.Filter.Regex(filtering.By, new BsonRegularExpression(string.Format("^{0}.*", filtering.Value), "i"));
                            totalFilter &= filter;
                            break;
                        case FilterOperator.EndsWidth:
                            filter = Builders<Tdb>.Filter.Regex(filtering.By, new BsonRegularExpression(string.Format(".*{0}$", filtering.Value), "i"));
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