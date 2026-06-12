using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using MongoTypeRepository.Contracts;
using Xunit;

namespace MongoTypeRepository.Tests
{
    public class GetPagedResultsAsyncTests
    {
        [Fact]
        public async Task GetPagedResultsAsync_AppliesSkipAndLimitServerSide()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            FindOptions<TestItem, TestItem>? captured = null;
            collection
                .Setup(c => c.FindAsync(It.IsAny<FilterDefinition<TestItem>>(),
                                        It.IsAny<FindOptions<TestItem, TestItem>>(),
                                        It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<TestItem>, FindOptions<TestItem, TestItem>, CancellationToken>(
                    (_, options, _) => captured = options)
                .ReturnsAsync(TestCursor.Empty<TestItem>());
            var repo = new TestRepository(collection.Object);
            await repo.GetPagedResultsAsync(null, new RepositoryPaging { CurrentPage = 3, PageSize = 20 });
            Assert.Equal(40, captured!.Skip);
            Assert.Equal(20, captured!.Limit);
        }

        // Sibling pin on the sync path so the two implementations can't diverge
        // again. The sync GetPagedResults goes through the IFindFluent built by
        // Collection.Find(...).Sort(...).Skip(...).Limit(...).ToList(), which
        // materializes by calling IMongoCollection.FindSync with an assembled
        // FindOptions<TestItem, TestItem> carrying the same Skip/Limit.
        [Fact]
        public void GetPagedResults_AppliesSkipAndLimitServerSide()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            FindOptions<TestItem, TestItem>? captured = null;
            collection
                .Setup(c => c.FindSync(It.IsAny<FilterDefinition<TestItem>>(),
                                       It.IsAny<FindOptions<TestItem, TestItem>>(),
                                       It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<TestItem>, FindOptions<TestItem, TestItem>, CancellationToken>(
                    (_, options, _) => captured = options)
                .Returns(TestCursor.Empty<TestItem>());
            var repo = new TestRepository(collection.Object);
            repo.GetPagedResults(null, new RepositoryPaging { CurrentPage = 3, PageSize = 20 });
            Assert.Equal(40, captured!.Skip);
            Assert.Equal(20, captured!.Limit);
        }
    }
}
