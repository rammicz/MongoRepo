using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoTypeRepository.Contracts;

namespace MongoTypeRepository
{
    public interface ITypeRepositoryBase<Tdb> where Tdb : IMongoItem
    {
        IMongoQueryable<Tdb> CollectionQuery { get; }
        Tdb GetById(string id);
        void Update(Tdb objectToSave);
        void Save(Tdb objectToSave);
        void Update(IEnumerable<Tdb> objectsToSave);
        void Insert(Tdb item);
        void Insert(IEnumerable<Tdb> items);
        void Delete(IMongoItem objectToDelete);
        void DeleteAll();
        List<Tdb> GetResults(FilterDefinition<Tdb> filter = null, RepositoryPaging paging = null, SortDefinition<Tdb> sort = null);
    }
}