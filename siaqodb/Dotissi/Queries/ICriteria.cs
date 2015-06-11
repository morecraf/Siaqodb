using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC_LMDB
using System.Threading.Tasks;
#endif
namespace Dotissi.Queries
{
    interface ICriteria
    {
        List<int> GetOIDs();
#if ASYNC_LMDB
        Task<List<int>> GetOIDsAsync();
#endif
    }
}
