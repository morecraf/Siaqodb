using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents.Indexes
{
    class IndexQueryFinder
    {
        public static List<string> FindKeys(IIndex index, Where query)
        {
            List<string> keys = new List<string>();
            List<string> keysFound = null;
            if (query.Operator == WhereOp.Equal)
            {
                keysFound = index.FindItem(query.Value);

            }
            else if (query.Operator == WhereOp.NotEqual)
            {
                keysFound = index.FindAllExcept(query.Value);
            }
            else if (query.Operator == WhereOp.Between)
            {
                keysFound =index.FindItemsBetween(query.Between[0], query.Between[1]);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.BetweenExceptStart)
            {
                keysFound = index.FindItemsBetweenExceptStart(query.Between[0], query.Between[1]);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.BetweenExceptEnd)
            {
                keysFound = index.FindItemsBetweenExceptEnd(query.Between[0], query.Between[1]);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.BetweenExceptStartEnd)
            {
                keysFound = index.FindItemsBetweenExceptStartEnd(query.Between[0], query.Between[1]);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.GreaterThanOrEqual)
            {
                keysFound = index.FindItemsBiggerThanOrEqual(query.Value);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.GreaterThan)
            {
                keysFound = index.FindItemsBiggerThan(query.Value);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.LessThanOrEqual)
            {
                keysFound = index.FindItemsLessThanOrEqual(query.Value);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.LessThan)
            {
                keysFound = index.FindItemsLessThan(query.Value);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.StartWith)
            {
                keysFound = index.FindItemsStartsWith(query.Value);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.EndWith)
            {
                keysFound = index.FindItemsEndsWith(query.Value);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
            }
            else if (query.Operator == WhereOp.Contains)
            {
                keysFound = index.FindItemsContains(query.Value);
                if (query.Descending == true)
                {
                    keysFound.Reverse();
                }
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
            return keys;
        }

        
    }
}
