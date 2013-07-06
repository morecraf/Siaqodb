using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Sqo
{
    public interface ISqoQuery<T>:IEnumerable<T>
    {
#if ASYNC
        Task<IList<T>> ToListAsync();
#endif
    }
    public interface ISqoOrderedQuery<T> : ISqoQuery<T>,IOrderedEnumerable<T>
    { 
#if ASYNC
        //Task<IOrderedEnumerable<T>> ToOrderedEnumerableAsync();
#endif
    }
}
