using System.Threading.Tasks;
using MongoTypeRepository;
using Xunit;

namespace MongoTypeRepository.Tests
{
    /// <summary>
    /// Green-from-day-one smoke test: proves the harness wires up and that the
    /// semaphore honours its maxCount (grants up to maxCount permits, blocks the
    /// next WaitAsync until a permit is released).
    /// </summary>
    public class ThrottlingSemaphoreSmokeTests
    {
        [Fact]
        public async Task WaitAsync_GrantsUpToMaxCount_ThenBlocks()
        {
            var semaphore = new ThrottlingSemaphore(2, 2);

            // First two permits are granted immediately.
            Assert.True(await semaphore.WaitAsync(0));
            Assert.True(await semaphore.WaitAsync(0));

            // At the limit: the next acquisition must block (times out).
            Assert.False(await semaphore.WaitAsync(50));

            // Releasing frees a permit so the next WaitAsync succeeds.
            semaphore.Release();
            Assert.True(await semaphore.WaitAsync(50));
        }
    }
}
