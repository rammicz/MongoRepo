using MongoTypeRepository.Example.CustomRepositories;

namespace MongoTypeRepository.Example.YourExistingTypes
{
    public class AlienShip
    {
        public string? Name { get; set; }
        public string? ComingFromPlanet { get; set; }
        public Alien[]? Crew { get; set; }
    }
}