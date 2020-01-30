using System.Configuration;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoTypeRepository.Example.DataModel;

namespace MongoTypeRepository.Example.CustomRepositories
{
    public class AlienShipsRepository : TypeRepositoryBase<WrappedAlienShip>
    {
        //public AlienShipsRepository() :  base(ConfigurationManager.ConnectionStrings["localMongo"].ConnectionString, "alienShips")
        public AlienShipsRepository() :  base("MongoTypeRepository", "alienShips", true)
        {
        }

        /// <summary>
        ///     demo only
        /// </summary>
        /// <returns></returns>
        public async Task<WrappedAlienShip> CustomGetAlienShip()
        {
            FilterDefinitionBuilder<WrappedAlienShip> filterBuilder = Builders<WrappedAlienShip>.Filter;

            FilterDefinition<WrappedAlienShip> filter = filterBuilder.Eq("Item.Crew.Name", "Dracula") & filterBuilder.Lte("YouCanDefineMorePropsHere", 10);

            return await Collection.Find(filter).FirstAsync();
        }
    }
}