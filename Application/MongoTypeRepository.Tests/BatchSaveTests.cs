using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace MongoTypeRepository.Tests
{
    public class BatchSaveTests
    {
        [Fact(Skip = "red: SaveAsync(IEnumerable) issues N ReplaceOneAsync calls instead of one BulkWrite")]
        public async Task SaveAsync_Batch_UsesSingleBulkWrite()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            var repo = new TestRepository(collection.Object);
            var items = Enumerable.Range(0, 5).Select(_ => new TestItem()).ToList();
            await repo.SaveAsync(items);
            collection.Verify(c => c.BulkWriteAsync(
                It.Is<IEnumerable<WriteModel<TestItem>>>(m =>
                    m.Count() == 5 && m.All(w => (w as ReplaceOneModel<TestItem>) != null && ((ReplaceOneModel<TestItem>)w).IsUpsert)),
                It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            collection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<TestItem>>(), It.IsAny<TestItem>(),
                It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
