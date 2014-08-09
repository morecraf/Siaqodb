using Cryptonor.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public static class CryptonorQueryExtensions 
    {
      

        public static ICryptonorQuery Configure(this ICryptonorQuery query, Action<QueryConfigurator> configurator)
        {
            configurator(new QueryConfigurator(query));

            return query;
        }
    }
}
