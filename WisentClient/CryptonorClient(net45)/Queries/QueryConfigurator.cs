using Cryptonor.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public class QueryConfigurator
    {
        protected readonly ICryptonorQuery Query;
        public QueryConfigurator(ICryptonorQuery query)
        {
            this.Query = query;
        }
        public QueryConfigurator Start(object value)
        {
            this.Query.Start = value;
            this.SetTagType(value);
            return this;
        }
        public QueryConfigurator End(object value)
        {
            this.Query.End = value;
            this.SetTagType(value);
            return this;
        }
        public QueryConfigurator Value(object value)
        {
            this.SetTagType(value);
            this.Query.Value = value;
            return this;
        }
        public QueryConfigurator In(object[] values)
        {
            this.SetTagType(values[0]);
            this.Query.In = values;
            return this;
        }
        public QueryConfigurator Start<T>(T value)
        {
            return this.Start((object)value);
        }
        public QueryConfigurator End<T>(T value)
        {
            return this.End((object)value);
        }
        public QueryConfigurator Value<T>(T value)
        {
            return this.Value((object)value);
        }
        public QueryConfigurator In<T>(params T[] values)
        {

            return this.In(values.Cast<object>().ToArray());
        }
        public QueryConfigurator Skip(int skip)
        {
            this.Query.Skip = skip;
            return this;
        }
        public QueryConfigurator Take(int limit)
        {
            this.Query.Limit = limit;
            return this;
        }
        public QueryConfigurator Descending()
        {
            this.Query.Descending = true;
            return this;
        }
        private void SetTagType(object obj)
        {
            Type t = obj.GetType();

            if (t == typeof(bool))
                this.Query.TagType = TypeBool;
            else if (t == typeof(DateTime))
                this.Query.TagType = TypeDateTime;
            else if (t == typeof(long) || t == typeof(int))
                this.Query.TagType = TypeInt;
            else if (t == typeof(float) || t == typeof(double))
                this.Query.TagType = TypeDouble;
            else if (t == typeof(string))
                this.Query.TagType = TypeString;

        }
        public const string TypeInt = "tags_int";
        public const string TypeString = "tags_string";
        public const string TypeDateTime = "tags_datetime";
        public const string TypeBool = "tags_bool";
        public const string TypeDouble = "tags_double";
    }
}
