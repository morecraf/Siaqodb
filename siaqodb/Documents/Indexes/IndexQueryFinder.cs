using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents.Indexes
{
    class IndexQueryFinder
    {
        public static void FindKeys(IIndex index, Query query, List<string> keys)
        {
            IEnumerable<string> keysFound = null;
            if (query.Value != null)
            {
                keysFound = index.FindItem(query.Value);

            }
            else if (query.Start != null && query.End != null)
            {
                keysFound = GetByStartEnd(query.Descending, query.Start, query.End, index);

            }
            else if (query.Start != null && query.End == null)
            {
                keysFound = GetByStart(query.Descending, query.Start, index);
            }
            else if (query.Start == null && query.End != null)
            {
                keysFound = GetByEnd(query.Descending, query.End, index);
            }
            else if (query.In != null)
            {
                foreach (object objTarget in query.In)
                {
                    var keysIn = index.FindItem(objTarget);
                    if (keysIn != null)
                    {
                        keys.AddRange(keysIn);

                    }
                }
            }
            if (keysFound != null)
            {
                keys.AddRange(keysFound);
            }
        }

        private static IEnumerable<string> GetByStartEnd(bool? desc, object start, object end, IIndex index)
        {
            List<string> oids = null;
            if (desc == true)
            {
                oids = index.FindItemsBetween(end, start);
                oids.Reverse();
            }
            else
            {
                oids = index.FindItemsBetween(start, end);
            }
            return oids;
        }
        private static List<string> GetByStart(bool? desc, object start, IIndex index)
        {
            if (desc == true)
            {
                var oids = index.FindItemsLessThanOrEqual(start);
                oids.Reverse();
                return oids;
            }
            else
            {
                var oids = index.FindItemsBiggerThanOrEqual(start);
                return oids;
            }
        }
        private static List<string> GetByEnd(bool? desc, object end, IIndex index)
        {
            if (desc == true)
            {
                var oids = index.FindItemsBiggerThanOrEqual(end);
                oids.Reverse();
                return oids;
            }
            else
            {
                return index.FindItemsLessThanOrEqual(end);
            }
        }
    }
}
