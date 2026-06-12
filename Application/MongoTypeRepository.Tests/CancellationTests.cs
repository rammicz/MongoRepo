using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace MongoTypeRepository.Tests
{
    public class CancellationTests
    {
        // Pre-cancelled token must throw before the driver is touched, even without a throttle wrapper.
        [Fact]
        public async Task FindAsync_PreCancelledToken_ThrowsWithoutInvokingDriver()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            var repo = new TestRepository(collection.Object);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => repo.FindAsync(FilterDefinition<TestItem>.Empty, cancellationToken: cts.Token));

            collection.Verify(c => c.FindAsync(It.IsAny<FilterDefinition<TestItem>>(),
                It.IsAny<FindOptions<TestItem, TestItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // The token handed to the repository must reach the underlying driver call.
        [Fact]
        public async Task FindAsync_ForwardsTokenToDriver()
        {
            var collection = new Mock<IMongoCollection<TestItem>>();
            CancellationToken captured = default;
            collection
                .Setup(c => c.FindAsync(It.IsAny<FilterDefinition<TestItem>>(),
                                        It.IsAny<FindOptions<TestItem, TestItem>>(),
                                        It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<TestItem>, FindOptions<TestItem, TestItem>, CancellationToken>(
                    (_, _, token) => captured = token)
                .ReturnsAsync(TestCursor.Empty<TestItem>());
            var repo = new TestRepository(collection.Object);
            using var cts = new CancellationTokenSource();

            await repo.FindAsync(FilterDefinition<TestItem>.Empty, cancellationToken: cts.Token);

            Assert.Equal(cts.Token, captured);
        }
    }
}
