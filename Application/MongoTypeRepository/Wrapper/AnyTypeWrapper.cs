using MongoDB.Bson;

namespace MongoTypeRepository.Wrapper
{
    public class AnyTypeWrapper<T> : IMongoItem
    {
        public AnyTypeWrapper(T item)
        {
            Item = item;
        }

        public AnyTypeWrapper(T item, ObjectId id) : this(item)
        {
            this.Id = id;
        } 

        public T Item { get; private set; }
        public ObjectId Id { get; set; }
    }
}