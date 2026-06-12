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

        public async Task<T> AddRequest<T>(Func<Task<T>> taskFactory, CancellationToken cancellationToken = default)
        {
            await this.WaitAsync(cancellationToken);
            try
            {
                return await taskFactory();
            }
            finally
            {
                this.Release();
            }
        }

        public async Task AddRequest(Func<Task> taskFactory, CancellationToken cancellationToken = default)
        {
            await this.WaitAsync(cancellationToken);
            try
            {
                await taskFactory();
            }
            finally
            {
                this.Release();
            }
        }
    }
}
