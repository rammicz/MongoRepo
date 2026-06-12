using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoTypeRepository.Tests
{
    /// <summary>
    /// Renders a <see cref="FilterDefinition{T}"/> to its underlying BSON using
    /// the driver's serializer registry - lets the paging-filter specs assert on
    /// the exact query document without a live server.
    /// </summary>
    public static class RenderHelper
    {
        public static BsonDocument Render<T>(FilterDefinition<T> filter)
        {
            var serializer = BsonSerializer.SerializerRegistry.GetSerializer<T>();
            var args = new RenderArgs<T>(serializer, BsonSerializer.SerializerRegistry);
            return filter.Render(args);
        }
    }
}
