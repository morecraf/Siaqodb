using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Sqo.Queries
{
    interface ICriteria
    {
        List<int> GetOIDs();
#if ASYNC
        Task<List<int>> GetOIDsAsync();
#endif
    }
}
