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
        private static bool _isDbSetUp;

        public TypeRepositoryBase(MongoUrl url, string databaseName, string collectionName) : this(databaseName, collectionName)
        {
            MongoClient = new MongoClient(url);
        }

        public TypeRepositoryBase(string connectionString, string databaseName, string collectionName) : this(databaseName, collectionName)
        {
            MongoClient = new MongoClient(connectionString);
        }

        public TypeRepositoryBase(MongoClientSettings mongoClientSettings, string databaseName, string collectionName) : this(databaseName, collectionName)
        {
            MongoClient = new MongoClient(mongoClientSettings);
        }

        public TypeRepositoryBase(string databaseName, string collectionName)
        {
            MongoClient = MongoClient ?? new MongoClient();
            IMongoDatabase db = MongoClient.GetDatabase(databaseName);
            Collection = db.GetCollection<Tdb>(collectionName);
        }

        protected IMongoCollection<Tdb> Collection { get; set; }
        protected MongoClient MongoClient { get; }

        public IMongoQueryable<Tdb> CollectionQuery
        {
            get { return Collection.AsQueryable(); }
        }

        public Tdb GetById(string id)
        {
            IMongoQueryable<Tdb> vystup = from obj in CollectionQuery
                where obj.Id.Equals(id)
                select obj;

            return vystup.FirstOrDefault();
        }

        public void Update(Tdb objectToSave)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToSave.Id));
            var updateOptions = new UpdateOptions { IsUpsert = false }; // update or insert / upsert
            Collection.ReplaceOne(filter, objectToSave);
        }

        public void Save(Tdb objectToSave)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToSave.Id));
            var updateOptions = new UpdateOptions { IsUpsert = true }; // update or insert / upsert
            Collection.ReplaceOne(filter, objectToSave, updateOptions);
        }

        public void Update(IEnumerable<Tdb> objectsToSave)
        {
            //var updateOptions = new UpdateOptions { IsUpsert = true }; // update or insert / upsert
            foreach (Tdb obj in objectsToSave)
            {
                FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", obj.Id));
                Collection.ReplaceOne(filter, obj);
            }
        }

        public void Insert(Tdb item)
        {
            Collection.InsertOne(item);
        }

        public void Insert(IEnumerable<Tdb> items)
        {
            if (items.Any())
            {
                Collection.InsertMany(items);
            }
        }

        public void Delete(IMongoItem objectToDelete)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToDelete.Id));
            Collection.DeleteOne(filter);
        }

        public void DeleteAll()
        {
            Collection.DeleteMany(FilterDefinition<Tdb>.Empty);
        }

        public List<Tdb> GetResults(FilterDefinition<Tdb> filter = null, RepositoryPaging paging = null, SortDefinition<Tdb> sort = null)
        {
            IFindFluent<Tdb, Tdb> filtered = Collection.Find(filter ?? JsonFilterDefinition<Tdb>.Empty);

            Task<long> totalTask = null;
            if (paging != null)
            {
                //count is on it's own cursor, so it can work in paralel
                totalTask = filtered.CountAsync();
            }

            if (sort != null)
            {
                filtered = filtered.Sort(sort);
            }

            if (paging.CurrentPage < 1)
            {
                paging.CurrentPage = 1;
            }

            if (paging.PageSize < 1)
            {
                paging.PageSize = 1;
            }

            // runs over cursor in MongoDb
            List<Tdb> items = filtered.Skip((paging.CurrentPage - 1) * paging.PageSize).Limit(paging.PageSize).ToList();

            if (totalTask != null)
            {
                totalTask.Wait();
                paging.TotalItems = totalTask.Result;
            }

            return items;
        }
    }
}