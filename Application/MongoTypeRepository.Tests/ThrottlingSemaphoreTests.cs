using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoTypeRepository;
using Xunit;

namespace MongoTypeRepository.Tests
{
    public class ThrottlingSemaphoreTests
    {
        [Fact]
        public async Task AddRequest_LimitsConcurrentExecution()
        {
            var semaphore = new ThrottlingSemaphore(2, 2);
            int running = 0, maxObserved = 0;
            var gate = new object();
            async Task Operation()
            {
                lock (gate) { running++; maxObserved = Math.Max(maxObserved, running); }
                await Task.Delay(50);
                lock (gate) { running--; }
            }
            var tasks = Enumerable.Range(0, 10).Select(_ => semaphore.AddRequest(() => Operation()));
            await Task.WhenAll(tasks);
            Assert.True(maxObserved <= 2, $"observed {maxObserved} concurrent operations, limit is 2");
        }

        [Fact]
        public async Task AddRequest_ReleasesPermitWhenOperationFaults()
        {
            var semaphore = new ThrottlingSemaphore(1, 1);
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => semaphore.AddRequest(() => Task.FromException(new InvalidOperationException("boom"))));
            var followUp = semaphore.AddRequest(() => Task.CompletedTask);
            var completed = await Task.WhenAny(followUp, Task.Delay(1000)) == followUp;
            Assert.True(completed, "permit leaked - semaphore deadlocked after a faulted operation");
        }

        [Fact]
        public async Task AddRequest_ReleasesPermitWhenFactoryThrowsSynchronously()
        {
            var semaphore = new ThrottlingSemaphore(1, 1);
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => semaphore.AddRequest(() => throw new InvalidOperationException("boom")));
            var followUp = semaphore.AddRequest(() => Task.CompletedTask);
            var completed = await Task.WhenAny(followUp, Task.Delay(1000)) == followUp;
            Assert.True(completed, "permit leaked - semaphore deadlocked after a synchronously-throwing factory");
        }

        [Fact]
        public async Task AddRequest_FactoryNotInvokedUntilPermitAcquired()
        {
            var semaphore = new ThrottlingSemaphore(1, 1);
            // Hold the only permit with a never-completing operation.
            var blocker = new TaskCompletionSource<bool>();
            _ = semaphore.AddRequest(() => blocker.Task);
            await Task.Yield();

            var factoryInvoked = false;
            var queued = semaphore.AddRequest(() =>
            {
                factoryInvoked = true;
                return Task.CompletedTask;
            });

            // The permit is held, so the queued factory must not have run yet.
            await Task.Delay(50);
            Assert.False(factoryInvoked, "factory ran before the permit was acquired (throttling is a no-op)");

            blocker.SetResult(true);
            await queued;
            Assert.True(factoryInvoked);
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
