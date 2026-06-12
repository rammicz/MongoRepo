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
        // #14: a pre-cancelled token must fast-fail at the repository boundary with
        // an OperationCanceledException BEFORE the driver is ever touched. The
        // ThrowIfCancellationRequested() at the top of each async repo method is what
        // guarantees this even when (as here) the injected collection is a raw mock
        // with no throttle wrapper to honour WaitAsync(cancelled).
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

        // A waiter queued on a saturated throttle (limit 1, slot held) must be released
        // with an OperationCanceledException when its token is cancelled, and the held
        // slot must be unaffected (the original holder keeps running and can complete).
        [Fact]
        public async Task AddRequest_TokenCancelledWhileQueued_UnblocksWaiter()
        {
            var semaphore = new ThrottlingSemaphore(1, 1);

            // Hold the only permit with a never-completing operation.
            var blocker = new TaskCompletionSource<bool>();
            var holder = semaphore.AddRequest(() => blocker.Task);
            await Task.Yield();

            using var cts = new CancellationTokenSource();
            var waiterFactoryInvoked = false;
            var waiter = semaphore.AddRequest(() =>
            {
                waiterFactoryInvoked = true;
                return Task.CompletedTask;
            }, cts.Token);

            // The slot is held, so the waiter is parked on WaitAsync, not running.
            await Task.Delay(50);
            Assert.False(waiter.IsCompleted, "waiter ran while the only permit was held");

            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiter);
            Assert.False(waiterFactoryInvoked, "cancelled waiter must not invoke its factory");

            // The held slot is unaffected: the original holder still completes cleanly.
            blocker.SetResult(true);
            var completed = await Task.WhenAny(holder, Task.Delay(1000)) == holder;
            Assert.True(completed, "the held slot was disturbed by the queued waiter's cancellation");
            await holder;
        }
    }
}
