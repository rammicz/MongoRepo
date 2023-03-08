using System;
using MongoDB.Bson;
using MongoTypeRepository.Example.CustomRepositories;
using MongoTypeRepository.Example.YourExistingTypes;
using MongoTypeRepository.Wrapper;

namespace MongoTypeRepository.Example.DataModel
{
    /// <summary>
    /// this data object is created by wrapping any object type and adding ID field, so the mongoDB needs are satisfied.
    /// </summary>
    public class WrappedAlienShip : AnyTypeWrapper<AlienShip>
    {
        public WrappedAlienShip(AlienShip alienShip) : base(alienShip)
        {
            //you should always handle the ID in wrapped items
            //you can also copy some of existing values from the object
            // e.g.: this.Id = alienShip.Identification;
            this.Id = ObjectId.GenerateNewId();
        }

        public int YouCanDefineMorePropsHere { get; set; }
        public int ForYourRepositoryNeeds { get; set; }
    }
}