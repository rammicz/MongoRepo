namespace MongoRepository.Wrapper
{
    public class AnyTypeWrapper<T> : IMongoItem
    {
        public AnyTypeWrapper(T item)
        {
            Item = item;
        }

        public AnyTypeWrapper(T item, string id) : this(item)
        {
            this.Id = id;
        } 

        public T Item { get; private set; }
        public string Id { get; set; }
    }
}