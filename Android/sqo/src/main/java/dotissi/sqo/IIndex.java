package dotissi.sqo;

import java.io.Closeable;
import java.util.ArrayList;

interface IIndex extends Closeable
        {
        ArrayList<String> FindItem(Object key);
        ArrayList<String> FindAllExcept(Object key);
        ArrayList<String> FindItemsBetween(Object start, Object end);
        ArrayList<String> FindItemsBetweenExceptStart(Object start, Object end);
        ArrayList<String> FindItemsBetweenExceptEnd(Object start, Object end);
        ArrayList<String> FindItemsBetweenExceptStartEnd(Object start, Object end);
        ArrayList<String> FindItemsBiggerThan(Object start);
        ArrayList<String> FindItemsBiggerThanOrEqual(Object start);
        ArrayList<String> FindItemsLessThan(Object start);
        ArrayList<String> FindItemsLessThanOrEqual(Object start);
        ArrayList<String> FindItemsStartsWith(Object target_key);
        ArrayList<String> FindItemsContains(Object target_key);
        ArrayList<String> FindItemsEndsWith(Object target_key);



        }