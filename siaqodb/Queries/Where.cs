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
namespace Sqo.Queries
{
    class Where :ICriteria
    {
        List<string> attributeName=new List<string>();
        public List<string> AttributeName
        {
            get { return attributeName; }
            set { attributeName = value; }
        }
        List<Type> parentType=new List<Type>();
        SqoTypeInfo ti;
        public List<Type> ParentType { get { return parentType; } set { parentType = value; } }
        public SqoTypeInfo ParentSqoTypeInfo { get { return ti; } set { ti = value; } }
       
        object val;
        public object Value
        {
            get { return val; } 
            set { val = value; } 
        }
        object val2;
        public object Value2
        {
            get { return val2; }
            set { val2 = value; }
        }
        OperationType opType;
        public OperationType OperationType { get { return opType; } set { opType = value; } }
        public Where(string fieldName,OperationType opType,object val)
        {
            this.attributeName.Add(fieldName);
            this.opType = opType;
            this.Value = val;
        }
        public Where()
        {

        }
        StorageEngine engine;
        public StorageEngine StorageEngine { get { return engine; } set { engine = value; } }
       
        #region ICriteria Members

        public List<int> GetOIDs()
        {
            List<int> oids = StorageEngine.LoadFilteredOids(this);
           
            return oids;
        }

        #endregion

#if ASYNC
        public async Task<List<int>> GetOIDsAsync()
        {
            List<int> oids = await StorageEngine.LoadFilteredOidsAsync(this).ConfigureAwait(false);

            return oids;
        }
#endif
    }
    internal enum OperationType { Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual ,StartWith,EndWith,Contains,ContainsKey,ContainsValue}
}
