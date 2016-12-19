using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoRepository;
using MongoTypeRepository.Example.DataModel;

namespace MongoTypeRepository.Example.CustomRepositories
{
    public class BooksRepository : TypeRepositoryBase<Book>
    {
        public BooksRepository() : base("MongoTypeRepository", "books")
        {
        }

        /// <summary>
        ///     This method is only for demo purposes
        /// </summary>
        public int GetBookCount()
        {
            Task<int> result = CollectionQuery.CountAsync();

            result.Wait();
            return result.Result;
        }
    }
}