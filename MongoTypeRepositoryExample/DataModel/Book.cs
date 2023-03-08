using System;
using MongoDB.Bson;

namespace MongoTypeRepository.Example.DataModel
{
    /// <summary>
    /// This data object is defined as you would expect, just use the IMongoItem interface
    /// </summary>
    public class Book : IMongoItem
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public DateTime DatePublished { get; set; }
        public BookChapter[] Chapters { get; set; }
        public ObjectId Id { get; set; }
    }
}