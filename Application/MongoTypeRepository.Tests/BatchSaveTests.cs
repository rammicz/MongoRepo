using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace MongoTypeRepository.Tests
{
    public class BatchSaveTests
    {
        [Fact]
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

        [Fact]
        public async Task UpdateAsync_Batch_UsesSingleBulkWrite_UpsertOff()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            var repo = new TestRepository(collection.Object);
            var items = Enumerable.Range(0, 4).Select(_ => new TestItem { Id = ObjectId.GenerateNewId() }).ToList();
            await repo.UpdateAsync(items);
            collection.Verify(c => c.BulkWriteAsync(
                It.Is<IEnumerable<WriteModel<TestItem>>>(m =>
                    m.Count() == 4 && m.All(w => (w as ReplaceOneModel<TestItem>) != null && !((ReplaceOneModel<TestItem>)w).IsUpsert)),
                It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            collection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<TestItem>>(), It.IsAny<TestItem>(),
                It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void Save_Batch_UsesSingleBulkWrite_UpsertOn_AndGeneratesMissingIds()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            var repo = new TestRepository(collection.Object);
            var items = Enumerable.Range(0, 3).Select(_ => new TestItem()).ToList();
            repo.Save(items);
            collection.Verify(c => c.BulkWrite(
                It.Is<IEnumerable<WriteModel<TestItem>>>(m =>
                    m.Count() == 3 && m.All(w => (w as ReplaceOneModel<TestItem>) != null && ((ReplaceOneModel<TestItem>)w).IsUpsert)),
                It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            collection.Verify(c => c.ReplaceOne(
                It.IsAny<FilterDefinition<TestItem>>(), It.IsAny<TestItem>(),
                It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.All(items, i => Assert.NotEqual(ObjectId.Empty, i.Id));
        }

        [Fact]
        public async Task SaveAsync_EmptyInput_PerformsZeroDriverCalls()
        {
            // MockBehavior.Strict throws on any unconfigured call - reaching the end proves zero driver calls.
            var collection = new Mock<IMongoCollection<TestItem>>(MockBehavior.Strict);
            var repo = new TestRepository(collection.Object);
            await repo.SaveAsync(new List<TestItem>());
            await repo.UpdateAsync(new List<TestItem>());
        }

        [Fact]
        public void Save_EmptyInput_PerformsZeroDriverCalls()
        {
            var collection = new Mock<IMongoCollection<TestItem>>(MockBehavior.Strict);
            var repo = new TestRepository(collection.Object);
            repo.Save(new List<TestItem>());
            repo.Update(new List<TestItem>());
        }
    }
}
