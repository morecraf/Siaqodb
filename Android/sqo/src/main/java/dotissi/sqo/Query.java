package dotissi.sqo;
import java.util.ArrayList;
public class Query
{
    ArrayList<Where> wheres = new ArrayList<Where>();
    Integer skip=-1;
    Integer limit=-1;
    ArrayList<SortableItem> orderby = new ArrayList<SortableItem>();
    ArrayList<Query> ors = new ArrayList<Query>();
    public Query()
    {

    }
    public Query whereEqual(String tagName, Object value)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName,value);
        w.Operator = WhereOp.Equal;
        wheres.add(w);
        return this;
    }
    public Query whereNotEqual(String tagName, Object value)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName,value);
        w.Operator = WhereOp.NotEqual;
        wheres.add(w);
        return this;
    }
    public Query whereGreaterThanOrEqual(String tagName, Object value)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName,value);
        w.Operator = WhereOp.GreaterThanOrEqual;
        wheres.add(w);
        return this;
    }

    public Query whereStartsWith(String tagName, String subString)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName, subString);
        w.Operator = WhereOp.StartWith;
        wheres.add(w);
        return this;
    }

    public Query whereEndsWith(String tagName, String subString)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName, subString);
        w.Operator = WhereOp.EndWith;
        wheres.add(w);
        return this;
    }

    public Query whereContains(String tagName, String subString)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName, subString);
        w.Operator = WhereOp.Contains;
        wheres.add(w);
        return this;
    }

    public Query whereGreaterThan(String tagName, Object value)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName, value);
        w.Operator = WhereOp.GreaterThan;
        wheres.add(w);
        return this;
    }
    public Query whereLessThan(String tagName, Object value)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName, value);
        w.Operator = WhereOp.LessThan;
        wheres.add(w);
        return this;
    }
    public Query whereLessThanOrEqual(String tagName, Object value)
    {
        Where w = new Where(tagName);
        w.Value = SetValue(tagName, value);
        w.Operator = WhereOp.LessThanOrEqual;
        wheres.add(w);
        return this;
    }
    public Query whereIN(String tagName, Object[] value)
    {
        Where w = new Where(tagName);
        w.In = SetValueArr(tagName, value);
        w.Operator = WhereOp.In;
        wheres.add(w);
        return this;
    }


    public Query whereBetween(String tagName, Object start,Object end)
    {
        Where w = new Where(tagName);
        w.Between = new Object[] { SetValue(tagName, start), SetValue(tagName, end) };
        w.Operator = WhereOp.Between;
        wheres.add(w);
        return this;
    }
    public Query limit(int limit)
    {
        this.limit = limit;
        return this;
    }
    public Query skip(int skip)
    {
        this.skip = skip;
        return this;
    }
    public Query orderBy(String tagName)
    {
        SortableItem si = new SortableItem();
        si.tagName = tagName;
        orderby.add(si);
        return this;
    }
    public Query orderByDesc(String tagName)
    {
        SortableItem si = new SortableItem();
        si.tagName = tagName;
        si.desc = true;
        orderby.add(si);
        return this;
    }

    public Query thenBy(String tagName)
    {
        SortableItem si = new SortableItem();
        si.tagName = tagName;
        orderby.add(si);
        return this;
    }
    public Query thenByDesc(String tagName)
    {
        SortableItem si = new SortableItem();
        si.tagName = tagName;
        si.desc = true;
        orderby.add(si);
        return this;
    }
    public Query or(Query query)
    {
        ors.add(query);
        return this;
    }
    protected Object SetValue(String tagName, Object obj) {
        Class<? extends Object> type = obj.getClass();

        if (type == int.class || type == long.class || type == Long.class || type == Integer.class) {
            return Long.parseLong(obj.toString());

        } else if (type == float.class || type == Float.class || type == double.class || type == Double.class) {
            return Double.parseDouble(obj.toString());
        }
        return obj;

    }
    private Object[] SetValueArr(String tagName,Object[] value)
    {
        for (int i = 0; i < value.length; i++)
        {
            value[i] = SetValue(tagName, value[i]);
        }
        return value;
    }

    class SortableItem
{
    public boolean desc;
    public String tagName;
}

}