using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents
{
    public class Query
    {
        internal List<Where> wheres = new List<Where>();
        internal int? skip;
        internal int? limit;
        internal List<SortableItem> orderby = new List<SortableItem>();
        internal List<Query> ors = new List<Query>();
        public Query()
        {

        }
        public Query WhereEqual(string tagName, object value)
        {
            Where w = new Where(tagName);
            w.Value = SetValue(value);
            w.Operator = WhereOp.Equal;
            wheres.Add(w);
            return this;
        }
        public Query WhereNotEqual(string tagName, object value)
        {
            Where w = new Where(tagName);
            w.Value = SetValue(value);
            w.Operator = WhereOp.NotEqual;
            wheres.Add(w);
            return this;
        }
        public Query WhereGreaterThanOrEqual(string tagName, object value)
        {
            Where w = new Where(tagName);
            w.Value = SetValue(value);
            w.Operator = WhereOp.GreaterThanOrEqual;
            wheres.Add(w);
            return this;
        }

        public Query WhereStartsWith(string tagName, string substring)
        {
            Where w = new Where(tagName);
            w.Value = substring;
            w.Operator = WhereOp.StartWith;
            wheres.Add(w);
            return this;
        }

        public Query WhereEndsWith(string tagName, string substring)
        {
            Where w = new Where(tagName);
            w.Value = substring;
            w.Operator = WhereOp.EndWith;
            wheres.Add(w);
            return this;
        }

        public Query WhereContains(string tagName, string substring)
        {
            Where w = new Where(tagName);
            w.Value = substring;
            w.Operator = WhereOp.Contains;
            wheres.Add(w);
            return this;
        }

        public Query WhereGreaterThan(string tagName, object value)
        {
            Where w = new Where(tagName);
            w.Value = SetValue(value);
            w.Operator = WhereOp.GreaterThan;
            wheres.Add(w);
            return this;
        }
        public Query WhereLessThan(string tagName, object value)
        {
            Where w = new Where(tagName);
            w.Value = SetValue(value);
            w.Operator = WhereOp.LessThan;
            wheres.Add(w);
            return this;
        }
        public Query WhereLessThanOrEqual(string tagName, object value)
        {
            Where w = new Where(tagName);
            w.Value = SetValue(value);
            w.Operator = WhereOp.LessThanOrEqual;
            wheres.Add(w);
            return this;
        }
        public Query WhereIN(string tagName, object[] value)
        {
            Where w = new Where(tagName);
            w.In = SetValueArr(value);
            w.Operator = WhereOp.In;
            wheres.Add(w);
            return this;
        }

       
        public Query WhereBetween(string tagName, object start,object end)
        {
            Where w = new Where(tagName);
            w.Between = new object[] { SetValue(start), SetValue(end) };
            w.Operator = WhereOp.Between;
            wheres.Add(w);
            return this;
        }
        public Query Limit(int limit)
        {
            this.limit = limit;
            return this;
        }
        public Query Skip(int skip)
        {
            this.skip = skip;
            return this;
        }
        public Query OrderBy(string tagName)
        {
            SortableItem si = new SortableItem();
            si.tagName = tagName;
            orderby.Add(si);
            return this;
        }
        public Query OrderByDesc(string tagName)
        {
            SortableItem si = new SortableItem();
            si.tagName = tagName;
            si.desc = true;
            orderby.Add(si);
            return this;
        }
       
        public Query ThenBy(string tagName)
        {
            SortableItem si = new SortableItem();
            si.tagName = tagName;
            orderby.Add(si);
            return this;
        }
        public Query ThenByDesc(string tagName)
        {
            SortableItem si = new SortableItem();
            si.tagName = tagName;
            si.desc = true;
            orderby.Add(si);
            return this;
        }
        public Query Or(Query query)
        {
            ors.Add(query);
            return this;
        }
        private object SetValue(object obj)
        {
            Type t = obj.GetType();
       
            if (t == typeof(long) || t == typeof(int))
            {
                return Convert.ToInt64(obj);

            }
            else if (t == typeof(float) || t == typeof(double))
            {

                return Convert.ToDouble(obj);
            }
            return obj;

        }
        private object[] SetValueArr(object[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                value[i] = SetValue(value[i]);
            }
            return value;
        }

        internal class SortableItem
        {
            public bool desc;
            public string tagName; 
        }
        
    }
}
