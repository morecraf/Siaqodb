using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public class QueryObject
    {
        public int Limit { get; set; }
        public long ContinuationToken { get; set; }
        public List<Criteria> Filter { get; set; }

    }
   
}
