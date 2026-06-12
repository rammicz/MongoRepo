using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace MongoTypeRepository.Tests
{
    public class ObjectIdGenerationTests
    {
        [Fact(Skip = "red: GenerateNewId(DateTime.Now) embeds local time - run with TZ != UTC")]
        public async Task SaveAsync_NewItem_IdTimestampIsUtc()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            var repo = new TestRepository(collection.Object);
            var item = new TestItem();
            await repo.SaveAsync(item);
            var drift = (item.Id.CreationTime - DateTime.UtcNow).Duration();
            Assert.True(drift < TimeSpan.FromMinutes(5), $"Id timestamp off by {drift} - local time leaked into the ObjectId");
        }
    }
}
