using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Sqo
{
    [System.Runtime.CompilerServices.TypeForwardedFrom("Sqo.ISqoQuery<>, SiaqodbPortable, Version=4.0.0.0")]
    public interface ISqoQuery<T>:IEnumerable<T>
    {
#if ASYNC
        Task<IList<T>> ToListAsync();
#endif
    }
     [System.Runtime.CompilerServices.TypeForwardedFrom("Sqo.ISqoOrderedQuery<>, SiaqodbPortable, Version=4.0.0.0")]
    public interface ISqoOrderedQuery<T> : ISqoQuery<T>,IOrderedEnumerable<T>
    { 
#if ASYNC
        //Task<IOrderedEnumerable<T>> ToOrderedEnumerableAsync();
#endif
    }
}
