using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Utilities
{
    class SqoSortableItem
    {
        public List<object> items = new List<object>();
        public int oid;
        public SqoSortableItem(int oid, object value)
        {
            this.oid = oid;
            this.items.Add( value);
        }
        public void Add(object value)
        {
            items.Add(value);
        }
        
    }
    class SortClass
    {
        public int index;
        public bool desc;
        public SortClass(int index,bool desc)
        {
            this.index = index;
            this.desc = desc;
        }
    }
    class SqoComparer<T> : IComparer<T> where T : SqoSortableItem
    {

        List<bool> sortOrder = new List<bool>();
       
        public SqoComparer(bool desc)
        {
            sortOrder.Add( desc);
        }
        public void AddOrder(bool desc)
        {

            sortOrder.Add(desc);
        }
        #region IComparer<T> Members

        public int Compare(T x, T y)
        {
            if (sortOrder.Count == 0)
            {
                return 0;
            }
            return CheckSort( x, y);
        }

        #endregion
        private int CheckSort(SqoSortableItem MyObject1, SqoSortableItem MyObject2)
        {
            int returnVal = 0;

            for(int i=0;i< MyObject1.items.Count;i++)
            {
                object valueOf1 = MyObject1.items[i];
                object valueOf2 = MyObject2.items[i];
                int result = ((IComparable)valueOf1).CompareTo((IComparable)valueOf2);
                if (result != 0)
                {
                    if (sortOrder[i])//if desc
                    {
                        return -result;
                    }
                    else
                    {
                        return result;
                    }
                }
                
            }
            return returnVal;
        }
    }

}
