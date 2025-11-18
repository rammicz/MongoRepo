using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Search;

namespace MongoTypeRepository
{
    public partial class ThrottledMongoCollection<T> : IMongoCollection<T>
    {
        //is a ... and has a ...
        private readonly IMongoCollection<T> _base;
        private readonly ThrottlingSemaphore _semaphore;

        public ThrottledMongoCollection(IMongoCollection<T> baseCollection, int connectionLimit)
        {
            _base = baseCollection ?? throw new ArgumentNullException(nameof(baseCollection));
            _semaphore = new ThrottlingSemaphore(connectionLimit, connectionLimit);
        }

        public IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.Aggregate(pipeline, options, cancellationToken);
        
        public IAsyncCursor<TResult> Aggregate<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.Aggregate(session, pipeline, options, cancellationToken);

        public async Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.AggregateAsync(pipeline, options, cancellationToken));

        public async Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.AggregateAsync(session, pipeline, options, cancellationToken));

        public void AggregateToCollection<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            _base.AggregateToCollection(pipeline, options, cancellationToken);
        }

        public void AggregateToCollection<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline,
            AggregateOptions options = null, CancellationToken cancellationToken = new CancellationToken())
        {
            _base.AggregateToCollection(session, pipeline, options, cancellationToken);
        }

        public async Task AggregateToCollectionAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await _semaphore.AddRequest(_base.AggregateToCollectionAsync(pipeline, options, cancellationToken));
        }

        public async Task AggregateToCollectionAsync<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline,
            AggregateOptions options = null, CancellationToken cancellationToken = new CancellationToken())
        {
            await _semaphore.AddRequest(_base.AggregateToCollectionAsync(session, pipeline, options, cancellationToken));
        }

        public BulkWriteResult<T> BulkWrite(IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.BulkWrite(requests, options, cancellationToken);

        public BulkWriteResult<T> BulkWrite(IClientSessionHandle session, IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.BulkWrite(session, requests, options, cancellationToken);

        public async Task<BulkWriteResult<T>> BulkWriteAsync(IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.BulkWriteAsync(requests, options, cancellationToken));

        public async Task<BulkWriteResult<T>> BulkWriteAsync(IClientSessionHandle session, IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.BulkWriteAsync(session, requests, options, cancellationToken));

        public long Count(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.Count(filter, options, cancellationToken);

        public long Count(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.Count(session, filter, options, cancellationToken);

        public async Task<long> CountAsync(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.CountAsync(filter, options, cancellationToken));

        public async Task<long> CountAsync(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.CountAsync(session, filter, options, cancellationToken));

        public long CountDocuments(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.CountDocuments(filter, options, cancellationToken);

        public long CountDocuments(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.CountDocuments(session, filter, options, cancellationToken);

        public async Task<long> CountDocumentsAsync(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.CountDocumentsAsync(filter, options, cancellationToken));

        public async Task<long> CountDocumentsAsync(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.CountDocumentsAsync(session, filter, options, cancellationToken));

        public DeleteResult DeleteMany(FilterDefinition<T> filter, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.DeleteMany(filter, cancellationToken);

        public DeleteResult DeleteMany(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.DeleteMany(filter, options, cancellationToken);

        public DeleteResult DeleteMany(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.DeleteMany(session, filter, options, cancellationToken);

        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DeleteManyAsync(filter, cancellationToken));

        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DeleteManyAsync(filter, options, cancellationToken));

        public async Task<DeleteResult> DeleteManyAsync(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DeleteManyAsync(session, filter, options, cancellationToken));

        public DeleteResult DeleteOne(FilterDefinition<T> filter, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.DeleteOne(filter, cancellationToken);

        public DeleteResult DeleteOne(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.DeleteOne(filter, options, cancellationToken);

        public DeleteResult DeleteOne(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.DeleteOne(session, filter, options, cancellationToken);

        public async Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DeleteOneAsync(filter, cancellationToken));

        public async Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DeleteOneAsync(filter, options, cancellationToken));

        public async Task<DeleteResult> DeleteOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DeleteOneAsync(session, filter, options, cancellationToken));

        public IAsyncCursor<TField> Distinct<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.Distinct(field, filter, options, cancellationToken);

        public IAsyncCursor<TField> Distinct<TField>(IClientSessionHandle session, FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.Distinct(session, field, filter, options, cancellationToken);

        public async Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DistinctAsync(field, filter, options, cancellationToken));

        public async Task<IAsyncCursor<TField>> DistinctAsync<TField>(
            IClientSessionHandle session,
            FieldDefinition<T, TField> field,
            FilterDefinition<T> filter,
            DistinctOptions options = null,
            CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DistinctAsync(session, field, filter, options, cancellationToken));

        public IAsyncCursor<TItem> DistinctMany<TItem>(FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.DistinctMany(field, filter, options, cancellationToken);

        public IAsyncCursor<TItem> DistinctMany<TItem>(IClientSessionHandle session, FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.DistinctMany(session, field, filter, options, cancellationToken);

        public async Task<IAsyncCursor<TItem>> DistinctManyAsync<TItem>(FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DistinctManyAsync(field, filter, options, cancellationToken));

        public async Task<IAsyncCursor<TItem>> DistinctManyAsync<TItem>(IClientSessionHandle session, FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.DistinctManyAsync(session, field, filter, options, cancellationToken));

        public long EstimatedDocumentCount(EstimatedDocumentCountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.EstimatedDocumentCount(options, cancellationToken);

        public async Task<long> EstimatedDocumentCountAsync(EstimatedDocumentCountOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.EstimatedDocumentCountAsync(options, cancellationToken));

        public IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.FindSync(filter, options, cancellationToken);

        public IAsyncCursor<TProjection> FindSync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.FindSync(session, filter, options, cancellationToken);

        public async Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.FindAsync(filter, options, cancellationToken));

        public async Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.FindAsync(session, filter, options, cancellationToken));

        public TProjection FindOneAndDelete<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.FindOneAndDelete(filter, options, cancellationToken);

        public TProjection FindOneAndDelete<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.FindOneAndDelete(session, filter, options, cancellationToken);

        public async Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.FindOneAndDeleteAsync(filter, options, cancellationToken));

        public async Task<TProjection> FindOneAndDeleteAsync<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            FindOneAndDeleteOptions<T, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.FindOneAndDeleteAsync(session, filter, options, cancellationToken));

        public TProjection FindOneAndReplace<TProjection>(FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.FindOneAndReplace(filter, replacement, options, cancellationToken);

        public TProjection FindOneAndReplace<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.FindOneAndReplace(session, filter, replacement, options, cancellationToken);

        public async Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.FindOneAndReplaceAsync(filter, replacement, options, cancellationToken));

        public async Task<TProjection> FindOneAndReplaceAsync<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            T replacement,
            FindOneAndReplaceOptions<T, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.FindOneAndReplaceAsync(session, filter, replacement, options, cancellationToken));

        public TProjection FindOneAndUpdate<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.FindOneAndUpdate(filter, update, options, cancellationToken);

        public TProjection FindOneAndUpdate<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.FindOneAndUpdate(session, filter, update, options, cancellationToken);

        public async Task<TProjection> FindOneAndUpdateAsync<TProjection>(
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            FindOneAndUpdateOptions<T, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.FindOneAndUpdateAsync(filter, update, options, cancellationToken));

        public async Task<TProjection> FindOneAndUpdateAsync<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            FindOneAndUpdateOptions<T, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.FindOneAndUpdateAsync(session, filter, update, options, cancellationToken));

        public void InsertOne(T document, InsertOneOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.InsertOne(document, options, cancellationToken);

        public void InsertOne(IClientSessionHandle session, T document, InsertOneOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.InsertOne(session, document, options, cancellationToken);

        [Obsolete("Use the new overload of InsertOneAsync with an InsertOneOptions parameter instead.")]
        public async Task InsertOneAsync(T document, CancellationToken cancellationToken) =>
            await _semaphore.AddRequest(_base.InsertOneAsync(document, cancellationToken));

        public async Task InsertOneAsync(T document, InsertOneOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.InsertOneAsync(document, options, cancellationToken));

        public async Task InsertOneAsync(IClientSessionHandle session, T document, InsertOneOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.InsertOneAsync(session, document, options, cancellationToken));

        public void InsertMany(IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.InsertMany(documents, options, cancellationToken);

        public void InsertMany(IClientSessionHandle session, IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.InsertMany(session, documents, options, cancellationToken);

        public async Task InsertManyAsync(IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.InsertManyAsync(documents, options, cancellationToken));

        public async Task InsertManyAsync(IClientSessionHandle session, IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.InsertManyAsync(session, documents, options, cancellationToken));
        
        [Obsolete("Use Aggregation pipeline instead.")]
        public IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.MapReduce(map,reduce, options, cancellationToken);

        [Obsolete("Use Aggregation pipeline instead.")]
        public IAsyncCursor<TResult> MapReduce<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.MapReduce(session, map, reduce, options, cancellationToken);

        [Obsolete("Use Aggregation pipeline instead.")]
        public async Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.MapReduceAsync(map, reduce, options, cancellationToken));

        [Obsolete("Use Aggregation pipeline instead.")]
        public async Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(
            IClientSessionHandle session,
            BsonJavaScript map,
            BsonJavaScript reduce,
            MapReduceOptions<T, TResult> options = null,
            CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.MapReduceAsync(session, map, reduce, options, cancellationToken));

        public IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : T => _base.OfType<TDerivedDocument>();
        public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, ReplaceOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.ReplaceOne(filter, replacement, options, cancellationToken);

        [Obsolete("Use the overload that takes a ReplaceOptions instead of an UpdateOptions.")]
        public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.ReplaceOne(filter, replacement, options, cancellationToken);

        public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, ReplaceOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.ReplaceOne(session, filter, replacement, options, cancellationToken);

        [Obsolete("Use the overload that takes a ReplaceOptions instead of an UpdateOptions.")]
        public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.ReplaceOne(session, filter, replacement, options, cancellationToken);

        public async Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, ReplaceOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.ReplaceOneAsync(filter, replacement, options, cancellationToken));

        [Obsolete("Use the overload that takes a ReplaceOptions instead of an UpdateOptions.")]
        public async Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.ReplaceOneAsync(filter, replacement, options, cancellationToken));

        public async Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, ReplaceOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.ReplaceOneAsync(session, filter, replacement, options, cancellationToken));

        [Obsolete("Use the overload that takes a ReplaceOptions instead of an UpdateOptions.")]
        public async Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.ReplaceOneAsync(session, filter, replacement, options, cancellationToken));

        public UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.UpdateMany(filter, update, options, cancellationToken);

        public UpdateResult UpdateMany(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.UpdateMany(session, filter, update, options, cancellationToken);

        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.UpdateManyAsync(filter, update, options, cancellationToken));

        public async Task<UpdateResult> UpdateManyAsync(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.UpdateManyAsync(session, filter, update, options, cancellationToken));

        public UpdateResult UpdateOne(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.UpdateOne(filter, update, options, cancellationToken);

        public UpdateResult UpdateOne(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.UpdateOne(session, filter, update, options, cancellationToken);

        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.UpdateOneAsync(filter, update, options, cancellationToken));

        public async Task<UpdateResult> UpdateOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.UpdateOneAsync(session, filter, update, options, cancellationToken));

        public IChangeStreamCursor<TResult> Watch<TResult>(PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.Watch(pipeline, options, cancellationToken);

        public IChangeStreamCursor<TResult> Watch<TResult>(IClientSessionHandle session, PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            _base.Watch(pipeline, options, cancellationToken);

        public async Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions options = null, CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.WatchAsync(pipeline, options, cancellationToken));

        public async Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = new CancellationToken()) =>
            await _semaphore.AddRequest(_base.WatchAsync(session, pipeline, options, cancellationToken));

        public IMongoCollection<T> WithReadConcern(ReadConcern readConcern) =>
            _base.WithReadConcern(readConcern);

        public IMongoCollection<T> WithReadPreference(ReadPreference readPreference) =>
            _base.WithReadPreference(readPreference);

        public IMongoCollection<T> WithWriteConcern(WriteConcern writeConcern) =>
            _base.WithWriteConcern(writeConcern);

        public CollectionNamespace CollectionNamespace => _base.CollectionNamespace;
        public IMongoDatabase Database => _base.Database;
        public IBsonSerializer<T> DocumentSerializer => _base.DocumentSerializer;
        public IMongoIndexManager<T> Indexes => _base.Indexes;
        public MongoCollectionSettings Settings => _base.Settings;
        public IMongoSearchIndexManager SearchIndexes => _base.SearchIndexes;

        IMongoSearchIndexManager IMongoCollection<T>.SearchIndexes => _base.SearchIndexes;
    }
}