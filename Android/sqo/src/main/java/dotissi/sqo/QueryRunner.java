package dotissi.sqo;

import org.fusesource.lmdbjni.Transaction;

import java.lang.reflect.Array;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Set;


public class QueryRunner {

    TagsIndexManager indexManag;
    Transaction lmdbTransaction;
    String BucketName;
    TransactionManager transactionManager;
    public QueryRunner(TagsIndexManager indexManag, Transaction lmdbTransaction, String bucketName,TransactionManager transactionManager) {
        this.indexManag = indexManag;
        this.lmdbTransaction = lmdbTransaction;
        this.BucketName = bucketName;
        this.transactionManager=transactionManager;
    }

    public ArrayList<String> run(Query query) {
        if (query.wheres.size() == 0)
        {
            throw new IllegalArgumentException("Query does not have defined any filtering");
        }
        ArrayList<Where> uniqueWheres = new ArrayList<Where>();
        uniqueWheres.addAll(query.wheres);
        this.TryOptimizeBetween(query, uniqueWheres);
        this.MakeSorting(query, uniqueWheres);

        //intersection
        ArrayList<String> keys1 = GetBySingleWhere(uniqueWheres.get(0));
        for (int i = 1; i < uniqueWheres.size(); i++)
        {
            if (keys1.size() == 0)
                break;
            ArrayList<String>  keys2 = GetBySingleWhere(uniqueWheres.get(i));
            keys1.retainAll(keys2);
        }
        //TODO optimize this
        //union
        for(Query or:query.ors)
        {
            ArrayList<String> keys2 = this.run(or);
            HashSet<String> set=new HashSet<String>();
            set.addAll(keys1);

            for(String key: keys2)
            {
               set.add(key);
            }
            keys1=new ArrayList<String>();
            keys1.addAll(set);
        }
        return keys1;

    }
    private void TryOptimizeBetween(Query query, ArrayList<Where> uniqueWheres) {
        HashMap<String, Where> groups = new HashMap<String, Where>();
        ArrayList<Where> tobeRemoved=new ArrayList<>();
        ArrayList<Where> tobeAdded=new ArrayList<>();

        for (Where w : uniqueWheres) {
            if (!groups.containsKey(w.TagName)) {
                groups.put(w.TagName, w);
            } else {
                Where dup = groups.get(w.TagName);
                if ((w.Operator == WhereOp.GreaterThan || w.Operator == WhereOp.GreaterThanOrEqual) &&
                        (dup.Operator == WhereOp.LessThan || dup.Operator == WhereOp.LessThanOrEqual)) {
                    tobeRemoved.add(w);
                    tobeRemoved.add(dup);
                    Where wNew = new Where(w.TagName);
                    wNew.Between = new Object[] { w.Value, dup.Value };
                    wNew.Operator = this.getBetweenOperator(w.Operator, dup.Operator);
                    tobeAdded.add(wNew);
                }
                else if ((dup.Operator == WhereOp.GreaterThan || dup.Operator == WhereOp.GreaterThanOrEqual) &&
                        (w.Operator == WhereOp.LessThan || w.Operator == WhereOp.LessThanOrEqual)) {

                    tobeRemoved.add(w);
                    tobeRemoved.add(dup);
                    Where wNew = new Where(w.TagName);
                    wNew.Between = new Object[] { dup.Value, w.Value };
                    wNew.Operator = this.getBetweenOperator(dup.Operator, w.Operator);
                    tobeAdded.add(wNew);
                }
                groups.put(w.TagName,w);

            }
        }

    }
    private void MakeSorting(Query query, ArrayList<Where> uniqueWheres)
    {
        for (int i = 0; i < query.orderby.size(); i++)
        {
            int index =this.GetIndexForTagName(uniqueWheres, query.orderby.get(i).tagName);
            if (index >= 0)
            {
                uniqueWheres.get(index).Descending = query.orderby.get(i).desc;
                if (index != i)
                {
                    Where w = uniqueWheres.get(index);
                    uniqueWheres.remove(index);
                    uniqueWheres.add(i, w);
                }

            }
        }
    }
    private int GetIndexForTagName(ArrayList<Where> wheres,String tagName)
    {
        for(int i=0;i<wheres.size();i++)
        {
            if(wheres.get(i).TagName==tagName)
                return i;
        }
        return -1;
    }
    private ArrayList<String> GetBySingleWhere(Where where) {
        if (where.TagName == "key") {
            return this.LoadByKey(where, lmdbTransaction);
        } else //by tags
        {
            return this.indexManag.LoadKeysByIndex(where, this.BucketName, lmdbTransaction);
        }
    }

    private ArrayList<String> LoadByKey(Where where, Transaction lmdbTransaction) {
        IIndex index = new IndexKey(this.BucketName, lmdbTransaction,transactionManager);
        return IndexQueryFinder.FindKeys(index, where);
    }

    private WhereOp getBetweenOperator(WhereOp startOp, WhereOp endOp)
    {
        if (startOp == WhereOp.GreaterThan && endOp == WhereOp.LessThan)
            return WhereOp.BetweenExceptStartEnd;
        else if(startOp == WhereOp.GreaterThanOrEqual && endOp == WhereOp.LessThan)
            return WhereOp.BetweenExceptEnd;
        else if (startOp == WhereOp.GreaterThan && endOp == WhereOp.LessThanOrEqual)
            return WhereOp.BetweenExceptStart;
        else if (startOp == WhereOp.GreaterThanOrEqual && endOp == WhereOp.LessThanOrEqual)
            return WhereOp.Between;

        return WhereOp.Between;
    }
}
