using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoTypeRepository.Contracts;

namespace MongoTypeRepository
{
    public interface ITypeRepositoryBase<Tdb> where Tdb : IMongoItem
    {
        IMongoQueryable<Tdb> CollectionQuery { get; }
        Tdb GetById(string id);
        Tdb GetById(ObjectId id);
        void Update(Tdb objectToSave);
        void Save(Tdb objectToSave);
        void Update(IEnumerable<Tdb> objectsToSave);
        void Insert(Tdb item);
        void Insert(IEnumerable<Tdb> items);
        void Delete(IMongoItem objectToDelete);
        void Delete(string id);
        void Delete(ObjectId id);
        void DeleteAll();
        List<Tdb> GetPagedResults(FilterDefinition<Tdb> filter = null, RepositoryPaging paging = null);
    }
}