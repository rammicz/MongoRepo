using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoTypeRepository.Contracts;

namespace MongoTypeRepository
{
    public interface ITypeRepositoryBase<Tdb> where Tdb : IMongoItem
    {
        IMongoCollection<Tdb> Collection { get; }
        MongoClient MongoClient { get; }
        IQueryable<Tdb> CollectionQuery { get; }
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

        // NOTE: the trailing optional CancellationToken on the async members below is
        // source-compatible (existing call sites keep compiling) but BINARY-breaking:
        // recompilation is required against this assembly. Reserve removal/signature
        // changes for the next major version.
        Task<Tdb> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Tdb> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default);

        List<Tdb> Find(FilterDefinition<Tdb> filter);
        List<Tdb> Find(FilterDefinition<Tdb> filter, SortDefinition<Tdb> sort);
        List<Tdb> Find(FilterDefinition<Tdb> filter, SortDefinition<Tdb> sort, int? limit);
        Task<List<Tdb>> FindAsync(FilterDefinition<Tdb> filter, CancellationToken cancellationToken = default);
        Task<List<Tdb>> FindAsync(FilterDefinition<Tdb> filter, SortDefinition<Tdb> sort, CancellationToken cancellationToken = default);
        Task<List<Tdb>> FindAsync(FilterDefinition<Tdb> filter, SortDefinition<Tdb> sort, int? limit, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Replace or insert documents in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        void Save(IEnumerable<Tdb> objectsToSave);

        /// <summary>
        ///     Replace or insert documents in DB, based on _id
        /// </summary>
        Task SaveAsync(IEnumerable<Tdb> objectsToSave, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Replace or insert document in DB
        /// </summary>
        /// <param name="objectToSave"></param>
        Task SaveAsync(Tdb objectToSave, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Replaces documents in DB, based on _id
        /// </summary>
        /// <param name="objectsToSave"></param>
        Task UpdateAsync(IEnumerable<Tdb> objectsToSave, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Replaces document in DB, based on _id
        /// </summary>
        /// <param name="objectToSave"></param>
        Task<ReplaceOneResult> UpdateAsync(Tdb objectToSave, CancellationToken cancellationToken = default);

        Task InsertAsync(Tdb item, CancellationToken cancellationToken = default);
        Task InsertAsync(IEnumerable<Tdb> items, CancellationToken cancellationToken = default);
        Task<DeleteResult> DeleteAsync(IMongoItem objectToDelete, CancellationToken cancellationToken = default);
        Task<DeleteResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<DeleteResult> DeleteAsync(ObjectId id, CancellationToken cancellationToken = default);
        Task<DeleteResult> DeleteAllAsync(CancellationToken cancellationToken = default);
        Task<List<Tdb>> GetPagedResultsAsync(FilterDefinition<Tdb> primaryFilters, RepositoryPaging paging, CancellationToken cancellationToken = default);
    }
}