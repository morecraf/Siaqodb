using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents.Indexes
{
    interface IIndex : IDisposable
    {
        IEnumerable<string> FindItem(object key);
        List<string> FindItemsBetween(object start, object end);
        List<string> FindItemsBiggerThanOrEqual(object start);
        List<string> FindItemsLessThanOrEqual(object start);
    }
}
