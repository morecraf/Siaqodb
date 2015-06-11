using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC_LMDB
using System.Threading.Tasks;
#endif

namespace Dotissi.Core
{
    interface IByteTransformer
    {
        byte[] GetBytes(object obj);
        object GetObject(byte[] bytes);
#if ASYNC_LMDB
        Task<byte[]> GetBytesAsync(object obj);
        Task<object> GetObjectAsync(byte[] bytes);
#endif
    }
}
