using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Utilities;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo
{
    public class SqoOrderedQuery<T> : ISqoOrderedQuery<T>
    {

        internal List<SqoSortableItem> SortableItems { get; set; }
        
        internal Siaqodb siaqodb;
        internal SqoComparer<SqoSortableItem> comparer;
        internal SqoOrderedQuery(Siaqodb siaqodb, List<SqoSortableItem> sortableItems,SqoComparer<SqoSortableItem> comparer)
        {
            this.SortableItems = sortableItems;
            this.siaqodb = siaqodb;
            this.comparer = comparer;
        }

        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return this;
        }

        public List<int> SortAndGetOids()
        {
            this.SortableItems.Sort(this.comparer);

            List<int> oids = new List<int>(this.SortableItems.Count);
            foreach (SqoSortableItem item in this.SortableItems)
            {
                oids.Add(item.oid);
            }
            return oids;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            List<int> oids=this.SortAndGetOids();
            return new LazyEnumerator<T>(this.siaqodb, oids);

        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (IEnumerator<T>)this.GetEnumerator();
        }

        #endregion
#if ASYNC
        public async Task<IList<T>> ToListAsync()
        {
            List<int> oids = this.SortAndGetOids();
            
            IObjectList<T> list = new ObjectList<T>();
            ISqoAsyncEnumerator<T> asyncEnum = new LazyEnumerator<T>(this.siaqodb, oids);
            while (await asyncEnum.MoveNextAsync())
            {
                list.Add(asyncEnum.Current);
            }
            return list;
        }
#endif
    }
    public class SqoObjOrderedQuery<T> : ISqoOrderedQuery<T>
    {

        IOrderedEnumerable<T> query;
        internal SqoObjOrderedQuery(IOrderedEnumerable<T> query)
        {
            this.query = query;
        }
        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return query.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return query.GetEnumerator();
        }

        #endregion

        #region IOrderedEnumerable<T> Members

        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return query.CreateOrderedEnumerable<TKey>(keySelector, comparer, descending);
        }

        #endregion
#if ASYNC
        public async Task<IList<T>> ToListAsync()
        {
            ISqoOrderedQuery<T> querya = this.query as ISqoOrderedQuery<T>;
            if (querya != null)
            {
                return await querya.ToListAsync();
            }
            else
            {
                return this.query.ToList();
            }
        }
#endif
    }
    
}
