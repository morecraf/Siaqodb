using Cryptonor.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace CryptonorClient
{
    public static class CryptonorQueryExtensions 
    {
      

        public static ICryptonorQuery Setup(this ICryptonorQuery query, Action<QueryConfigurator> configurator)
        {
            configurator(new QueryConfigurator(query));

            return query;
        }
    }
}
