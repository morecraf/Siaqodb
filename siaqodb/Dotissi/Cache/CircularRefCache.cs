using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dotissi.Meta;

namespace Dotissi.Cache
{
    class CircularRefCache
    {
        class CircularRefChacheItem
        {
            public int OID { get; set; }
            public SqoTypeInfo TInfo { get; set; }
            public object Obj { get; set; }
        }
        private List<CircularRefChacheItem> list = new List<CircularRefChacheItem>();
        public void Add(int oid, SqoTypeInfo ti, object obj)
        {
            CircularRefChacheItem item= new CircularRefChacheItem { OID = oid, TInfo = ti, Obj = obj };
            list.Add(item);
        }
        public void Clear()
        {
            list.Clear();
        }
        public object GetObject(int oid, SqoTypeInfo ti)
        {
            foreach (CircularRefChacheItem item in list)
            {
                if (item.OID == oid && item.TInfo == ti)
                {
                    return item.Obj;
                        
                }
            }
            return null;
        }
    }
    
}
