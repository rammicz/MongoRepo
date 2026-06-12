using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace MongoTypeRepository.Tests
{
    public class CancellationTests
    {
        // TODO(#14): rewrite against the cancellation-aware overload once the
        // repository API grows a CancellationToken parameter. The target spec is:
        //
        //   await Assert.ThrowsAnyAsync<OperationCanceledException>(
        //       () => repo.FindAsync(FilterDefinition<TestItem>.Empty, cancellationToken: cts.Token));
        //
        // That overload does not exist yet, so the body below is written against
        // the current API purely so the project compiles; it stays skipped (red).
        [Fact(Skip = "red: repository API has no CancellationToken parameters")]
        public async Task FindAsync_PreCancelledToken_Throws()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            var repo = new TestRepository(collection.Object);
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAnyAsync<System.OperationCanceledException>(
                () => repo.FindAsync(FilterDefinition<TestItem>.Empty));
            collection.Verify(c => c.FindAsync(It.IsAny<FilterDefinition<TestItem>>(),
                It.IsAny<FindOptions<TestItem, TestItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
