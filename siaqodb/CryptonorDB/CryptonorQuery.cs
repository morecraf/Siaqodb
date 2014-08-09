using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonor.Queries
{
    public class CryptonorQuery :ICryptonorQuery
    {
        public CryptonorQuery(string tagOrKey)
        {
            this.TagName = tagOrKey;

        }
        public string TagName{get;set;}
        public object Value { get; set; }
        public object Start { get; set; }
        public object End { get; set; }
        public int? Skip { get; set; }
        public int? Limit { get; set; }
        public bool? Descending { get; set; }
        public string TagType { get; set; }
        public object[] In { get; set; }

        internal Type GetTagType()
        {
            if (TagType == TypeInt)
                return typeof(long);
            else if (TagType == TypeDateTime)
                return typeof(DateTime);
            else if (TagType == TypeString)
                return typeof(string);
            else if (TagType == TypeDouble)
                return typeof(double);
            else if (TagType == TypeBool)
                return typeof(bool);
            throw new Cryptonor.Exceptions.CryptonorException("Tag Type:" + TagType + " not supported! ");
         
        }
        internal const string TypeInt = "tags_int";
        internal const string TypeString = "tags_string";
        internal const string TypeDateTime = "tags_datetime";
        internal const string TypeBool = "tags_bool";
        internal const string TypeDouble = "tags_double";
    }
}
