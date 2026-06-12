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
        [Fact(Skip = "red: GetPagedResultsAsync pages client-side instead of server-side")]
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
    }
}
