using System;
using System.Linq;
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
    }
}
