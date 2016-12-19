using System;
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

        public void CreateShip()
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

            _alienRepo.Save(shipForDb);
        }

        public WrappedAlienShip GetByCustomRepositoryMethod()
        {
            return _alienRepo.CustomGetAlienShip();
        }

        private static Book CreateBook(string id, string name)
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