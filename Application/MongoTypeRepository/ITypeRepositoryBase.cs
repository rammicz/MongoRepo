using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoTypeRepository.Contracts;

namespace MongoTypeRepository
{
    public interface ITypeRepositoryBase<Tdb> where Tdb : IMongoItem
    {
        IMongoCollection<Tdb> Collection { get; }
        MongoClient MongoClient { get; }
        IMongoQueryable<Tdb> CollectionQuery { get; }
        Tdb GetById(string id);
        Tdb GetById(ObjectId id);

        /// <summary>
        ///     Replaces documents in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        void Update(IEnumerable<Tdb> objectsToSave);

        /// <summary>
        ///     Replaces document in DB, based on _id
        /// </summary>
        /// <param name="objectToSave"></param>
        ReplaceOneResult Update(Tdb objectToSave);

        void Insert(Tdb item);
        void Insert(IEnumerable<Tdb> items);
        void Delete(IMongoItem objectToDelete);
        void Delete(string id);
        void Delete(ObjectId id);
        DeleteResult DeleteAll();
        List<Tdb> GetPagedResults(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging);

        /// <summary>
        ///     Replace or insert document in DB
        /// </summary>
        /// <param name="objectToSave"></param>
        void Save(Tdb objectToSave);

        Task<Tdb> GetByIdAsync(string id);
        Task<Tdb> GetByIdAsync(ObjectId id);

        /// <summary>
        ///     Replace or insert documents in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        void Save(IEnumerable<Tdb> objectsToSave);

        /// <summary>
        ///     Replace or insert documents in DB, based on _id
        /// </summary>
        Task SaveAsync(IEnumerable<Tdb> objectsToSave);

        /// <summary>
        ///     Replace or insert document in DB
        /// </summary>
        /// <param name="objectToSave"></param>
        Task SaveAsync(Tdb objectToSave);

        /// <summary>
        ///     Replaces documents in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        Task UpdateAsync(IEnumerable<Tdb> objectsToSave);

        /// <summary>
        ///     Replaces document in DB, based on _id
        /// </summary>
        /// <param name="objectToSave"></param>
        Task<ReplaceOneResult> UpdateAsync(Tdb objectToSave);

        void InsertAsync(Tdb item);
        Task InsertAsync(IEnumerable<Tdb> items);
        Task<DeleteResult> DeleteAsync(IMongoItem objectToDelete);
        Task<DeleteResult> DeleteAsync(string id);
        Task<DeleteResult> DeleteAsync(ObjectId id);
        Task<DeleteResult> DeleteAllAsync();
        Task<List<Tdb>> GetPagedResultsAsync(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging);
    }
}