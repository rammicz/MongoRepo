using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoTypeRepository
{
    public class ThrottlingSemaphore : SemaphoreSlim
    {
        public ThrottlingSemaphore(int initialCount) : base(initialCount)
        {
        }

        public ThrottlingSemaphore(int initialCount, int maxCount) : base(initialCount, maxCount)
        {
        }

        public async Task<T> AddRequest<T>(Task<T> task)
        {
            await this.WaitAsync();
            var result = await task;
            this.Release();

            return result;
        }

        public async Task AddRequest(Task task)
        {
            await this.WaitAsync();
            await task;
            this.Release();
        }

        // Func-based overloads: these exist as a compile-time seam so the
        // sibling test specs (#9) can be written against the planned API.
        // NOTE: semantics intentionally mirror the existing hot-task overloads
        // (the factory is invoked eagerly, before WaitAsync) so this does NOT
        // fix the throttling no-op bug tracked by #9 - that is its own issue.
        public Task<T> AddRequest<T>(Func<Task<T>> taskFactory)
        {
            return AddRequest(taskFactory());
        }

        public Task AddRequest(Func<Task> taskFactory)
        {
            return AddRequest(taskFactory());
        }
    }
}
