package dotissi.sqo;

import java.util.ArrayList;
import java.util.Collections;

/**
 * Created by Cristi on 3/17/2016.
 */
public class IndexQueryFinder {
    public static ArrayList<String> FindKeys(IIndex index, Where query)
    {

        ArrayList<String>  keys = new ArrayList<String>();
        ArrayList<String>  keysFound = null;
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
        }
        else if (query.Operator == WhereOp.BetweenExceptStart)
        {
            keysFound = index.FindItemsBetweenExceptStart(query.Between[0], query.Between[1]);
        }
        else if (query.Operator == WhereOp.BetweenExceptEnd)
        {
            keysFound = index.FindItemsBetweenExceptEnd(query.Between[0], query.Between[1]);
        }
        else if (query.Operator == WhereOp.BetweenExceptStartEnd)
        {
            keysFound = index.FindItemsBetweenExceptStartEnd(query.Between[0], query.Between[1]);

        }
        else if (query.Operator == WhereOp.GreaterThanOrEqual)
        {
            keysFound = index.FindItemsBiggerThanOrEqual(query.Value);

        }
        else if (query.Operator == WhereOp.GreaterThan)
        {
            keysFound = index.FindItemsBiggerThan(query.Value);

        }
        else if (query.Operator == WhereOp.LessThanOrEqual)
        {
            keysFound = index.FindItemsLessThanOrEqual(query.Value);

        }
        else if (query.Operator == WhereOp.LessThan)
        {
            keysFound = index.FindItemsLessThan(query.Value);

        }
        else if (query.Operator == WhereOp.StartWith)
        {
            keysFound = index.FindItemsStartsWith(query.Value);

        }
        else if (query.Operator == WhereOp.EndWith)
        {
            keysFound = index.FindItemsEndsWith(query.Value);

        }
        else if (query.Operator == WhereOp.Contains)
        {
            keysFound = index.FindItemsContains(query.Value);

        }
        else if (query.In != null)
        {
            for (Object objTarget : query.In)
            {
                ArrayList<String> keysIn = index.FindItem(objTarget);
                if (keysIn != null)
                {
                    keys.addAll(keysIn);

                }
            }
        }
        if (keysFound != null)
        {
            if (query.Descending == true)
            {
                Collections.reverse(keysFound);
            }
            keys.addAll(keysFound);
        }
        return keys;
    }
}
