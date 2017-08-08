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
        public TypeRepositoryBase(MongoUrl url, string collectionName)
        {
            MongoClient = new MongoClient(url);
            SetUp(url.DatabaseName, collectionName);
        }

        public TypeRepositoryBase(string connectionString, string collectionName)
        {
            MongoUrl url = MongoUrl.Create(connectionString);
            MongoClient = new MongoClient(connectionString);
            SetUp(url.DatabaseName, collectionName);
        }

        public TypeRepositoryBase(string databaseName, string collectionName, bool isLocal)
        {
            if(!isLocal)
            { throw new Exception("This method can run only on local DB");}

            MongoClient = new MongoClient();
            SetUp(databaseName, collectionName);
        }

        public IMongoCollection<Tdb> Collection { get; private set; }
        public MongoClient MongoClient { get; }
        public IMongoQueryable<Tdb> CollectionQuery => Collection.AsQueryable();


        public Tdb GetById(string id)
        {
            IMongoQueryable<Tdb> vystup = from obj in CollectionQuery
                where obj.Id.Equals(id)
                select obj;

            return vystup.FirstOrDefault();
        }

        /// <summary>
        /// Replace or insert document in DB, based on _id
        /// </summary>
        /// <param name="objectToSave"></param>
        public void Save(IEnumerable<Tdb> objectsToSave)
        {
            foreach (Tdb objectToSave in objectsToSave)
            {
                Save(objectToSave);
            }
        }       
        
        /// <summary>
        /// Replace or insert document in DB
        /// </summary>
        /// <param name="objectToSave"></param>
        public void Save(Tdb objectToSave)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToSave.Id));
            var updateOptions = new UpdateOptions { IsUpsert = true }; // update or insert / upsert
            Collection.ReplaceOne(filter, objectToSave, updateOptions);
        }

        /// <summary>
        /// Replaces documents in DB, based on _id
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
        /// Replaces document in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        public void Update(Tdb objectToSave)
        {
            FilterDefinition<Tdb> filter = new BsonDocumentFilterDefinition<Tdb>(new BsonDocument("_id", objectToSave.Id));
            var updateOptions = new UpdateOptions { IsUpsert = false }; // update or insert / upsert
            Collection.ReplaceOne(filter, objectToSave, updateOptions);
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

        public List<Tdb> GetPagedResults(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging)
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

            IFindFluent<Tdb, Tdb> filtered = Collection.Find(totalFilter);

            //count is on it's own cursor, so it can work in paralel
            Task<long> totalTask = filtered.CountAsync();

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

            totalTask.Wait();
            paging.TotalItems = totalTask.Result;
            
            return items;
        }

        private void SetUp(string databaseName, string collectionName)
        {
            IMongoDatabase db = MongoClient.GetDatabase(databaseName);
            Collection = db.GetCollection<Tdb>(collectionName);
        }
    }
}