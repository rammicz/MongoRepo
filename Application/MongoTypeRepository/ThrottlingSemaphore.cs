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

    }
}
