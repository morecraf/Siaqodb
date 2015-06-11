#if ASYNC_LMDB
using Sqo.MetaObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sqo
{
    /// <summary>
    /// Defines a critical section with a mutual-exclusion lock.
    /// </summary>
    public class AsyncLock
    {
#if !SL4 && !MANGO  && !NET40
        private readonly SemaphoreSlim _semaphore;
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock" /> class.
        /// </summary>
        public AsyncLock()
        {
#if !SL4 && !MANGO  && !NET40
            _semaphore = new SemaphoreSlim(initialCount:1);
#endif
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Task LockAsync(Type tobj,out bool locked)
        {
#if !SL4 && !MANGO && !NET40
            if (tobj == null)
            {
                locked = true;
                return LockAsync();
            }
            if ((tobj.IsGenericType() && tobj.GetGenericTypeDefinition() == typeof(Dotissi.Indexes.BTreeNode<>)) 
                || tobj==typeof(Dotissi.Indexes.IndexInfo2)
                || tobj==typeof(RawdataInfo))
            {
                locked = false;
                return Task.Delay(0);
            }
            else
            {
                locked = true;
                return _semaphore.WaitAsync();
            }
#else
            locked = false;
            TaskCompletionSource<int> tcs1 = new TaskCompletionSource<int>();
            tcs1.SetResult(0);
            return tcs1.Task;
#endif
           
        }
        public Task LockAsync()
        {
#if !SL4 && !MANGO && !NET40
            return _semaphore.WaitAsync();
#else
            TaskCompletionSource<int> tcs1 = new TaskCompletionSource<int>();
            tcs1.SetResult(0);
            return tcs1.Task;
            #endif
        }
        public int Release()
        {
#if !SL4 && !MANGO && !NET40
            return _semaphore.Release();
#else
            return 0;
#endif
        }
    }
}
#endif
