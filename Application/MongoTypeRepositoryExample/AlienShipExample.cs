using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoTypeRepository.Example.CustomRepositories;
using MongoTypeRepository.Example.DataModel;
using MongoTypeRepository.Example.YourExistingTypes;

namespace MongoTypeRepository.Example
{
    public class AlienShipExample
    {
        private readonly AlienShipsRepository _alienRepo;

        public AlienShipExample()
        {
            _alienRepo = new AlienShipsRepository();
        }

        public async Task CreateShip()
        {
            var alienShip = new AlienShip
            {
                ComingFromPlanet = "Vampiris",
                Name = "Garlic",
                Crew = new[]
                {
                    new Alien
                    {
                        Name = "Dracula",
                        IsBloodThirsty = true
                    }
                }
            };

            var shipForDb = new WrappedAlienShip(alienShip);

            await _alienRepo.SaveAsync(shipForDb);
        }

        public async Task<WrappedAlienShip> GetByCustomRepositoryMethod()
        {
            return await _alienRepo.CustomGetAlienShip();
        }

        private static Book CreateBook(ObjectId id, string name)
        {
            return new Book
            {
                Id = id,
                Author = "Jiri Hernik",
                DatePublished = new DateTime(2016, 12, 19),
                Name = name,
                Chapters = new[]
                {
                    new BookChapter
                    {
                        Name = "Introduction",
                        Content = "Lorem ipsum"
                    }
                }
            };
        }
    }
}