using MongoDB.Driver;
using MongoRepository;
using MongoTypeRepository.Example.DataModel;

namespace MongoTypeRepository.Example.CustomRepositories
{
    public class AlienShipsRepository : TypeRepositoryBase<WrappedAlienShip>
    {
        public AlienShipsRepository() : base("MongoTypeRepository", "alienShips")
        {
        }

        /// <summary>
        ///     demo only
        /// </summary>
        /// <returns></returns>
        public WrappedAlienShip CustomGetAlienShip()
        {
            FilterDefinitionBuilder<WrappedAlienShip> filterBuilder = Builders<WrappedAlienShip>.Filter;

            FilterDefinition<WrappedAlienShip> filter = filterBuilder.Eq("Item.Crew.Name", "Dracula") & filterBuilder.Lte("YouCanDefineMorePropsHere", 10);

            return Collection.Find(filter).First();
        }
    }
}