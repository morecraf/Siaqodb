using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Sqo.Queries;
using Sqo.Meta;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace CryptonorClient
{
    class Where 
    {
        public string TagName { get; set; }
        public TagType TagType { get; set; }
       
        object val;
        public object TagValue
        {
            get { return val; } 
            set { val = value; } 
        }
        object val2;
        public object TagValue2
        {
            get { return val2; }
            set { val2 = value; }
        }
        OperationType opType;
        public OperationType OperationType { get { return opType; } set { opType = value; } }
        public Where(string tagName,OperationType opType,object val)
        {
            this.TagName = tagName;
            this.opType = opType;
            this.TagValue = val;
        }
        public Where()
        {

        }
       

    }
    internal enum OperationType { Equal, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,Between }
    internal enum TagType { Tags_Int, Tags_DateTime, Tags_String, Tags_Double, Tags_Bool }
}
