using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public class CryptonorResultSet
    {
        public long ContinuationToken { get; set; }
        public int Count { get; set; }
        public IList<CryptonorObject> Objects { get; set; }
    }
}
