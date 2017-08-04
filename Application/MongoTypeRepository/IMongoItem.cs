using MongoDB.Bson;

namespace MongoTypeRepository
{
    public interface IMongoItem
    {
        ObjectId Id { get; set; }
    }
}