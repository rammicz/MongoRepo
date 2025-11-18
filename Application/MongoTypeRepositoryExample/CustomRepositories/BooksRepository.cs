using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoTypeRepository.Example.DataModel;

namespace MongoTypeRepository.Example.CustomRepositories
{
    public class BooksRepository : TypeRepositoryBase<Book>
    {
        //public BooksRepository() :  base(ConfigurationManager.ConnectionStrings["localMongo"].ConnectionString, "books")
        public BooksRepository() :  base("MongoTypeRepository", "books", true)
        {
        }

        /// <summary>
        ///     This method is only for demo purposes
        /// </summary>
        public async Task<int> GetBookCountAsync()
        {
            return await ((MongoDB.Driver.Linq.IMongoQueryable<Book>)CollectionQuery).CountAsync();
        }
    }
}