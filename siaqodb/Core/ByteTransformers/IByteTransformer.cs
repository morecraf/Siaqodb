using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Core
{
    interface IByteTransformer
    {
        byte[] GetBytes(object obj);
        object GetObject(byte[] bytes);
#if ASYNC
        Task<byte[]> GetBytesAsync(object obj);
        Task<object> GetObjectAsync(byte[] bytes);
#endif
    }
}
