using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
using Sqo.Exceptions;
#endif

namespace Sqo
{
    public class SelectQueryWhere<T> : ISqoQuery<T>
    {
        IEnumerable<T> enumerator;
        Func<T, bool> predicate;
        ISqoQuery<T> query;
        public SelectQueryWhere(Func<T, bool> predicate, ISqoQuery<T> query)
        {
            this.predicate = predicate;
            this.query = query;
        }
#if ASYNC
        public async Task<IList<T>> ToListAsync()
        {
            IList<T> list = await query.ToListAsync();
            if (this.predicate != null)
            {
                this.enumerator = Enumerable.Where<T>(list, predicate);
                return this.enumerator.ToList<T>();
            }

            return null;
        }
#endif
        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (this.enumerator == null)
            {
                this.enumerator = Enumerable.Where<T>(this.query, predicate);
            }
            return enumerator.GetEnumerator();

        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (IEnumerator<T>)this.GetEnumerator();
        }

        #endregion

    }
    public class SelectQuery<T, V> : ISqoQuery<V>
    {
        IEnumerable<V> enumerator;
        Func<T, V> selector;
        ISqoQuery<T> query;
        public SelectQuery(Func<T, V> selector, ISqoQuery<T> query)
        {
            this.selector = selector;
            this.query = query;
        }
#if ASYNC
        public async Task<IList<V>> ToListAsync()
        {
            IList<T> list = await query.ToListAsync();
            if (this.selector != null)
            {
                this.enumerator = Enumerable.Select<T, V>(list, selector);
                return this.enumerator.ToList<V>();
            }

            return null;
        }
#endif
        #region IEnumerable<T> Members

        public IEnumerator<V> GetEnumerator()
        {
            if (this.enumerator == null)
            {
                this.enumerator = Enumerable.Select<T, V>(this.query, selector);
            }
            return enumerator.GetEnumerator();

        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (IEnumerator<V>)this.GetEnumerator();
        }

        #endregion

    }
    public class SelectQueryJoin<TOuter, TInner, TKey, TResult> : ISqoQuery<TResult>
    {
        IEnumerable<TResult> enumerator;
        ISqoQuery<TOuter> query;
        IEnumerable<TInner> inner;
        Func<TOuter, TKey> outerKeySelector;
         Func<TInner, TKey> innerKeySelector;
        Func<TOuter, TInner, TResult> resultSelector;
        public SelectQueryJoin(ISqoQuery<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            this.inner = inner;
            this.outerKeySelector = outerKeySelector;
            this.innerKeySelector = innerKeySelector;
            this.resultSelector = resultSelector;
            this.query = outer;
        }
#if ASYNC
        public async Task<IList<TResult>> ToListAsync()
        {
            IList<TOuter> list = await query.ToListAsync();
            if (this.resultSelector != null)
            {
                this.enumerator = Enumerable.Join<TOuter, TInner, TKey, TResult>(list, inner, outerKeySelector, innerKeySelector, resultSelector);
                return this.enumerator.ToList<TResult>();
            }

            return null;
        }
#endif
        #region IEnumerable<T> Members

        public IEnumerator<TResult> GetEnumerator()
        {
            if (this.enumerator == null)
            {
                this.enumerator = Enumerable.Join<TOuter, TInner, TKey, TResult>(query, inner, outerKeySelector, innerKeySelector, resultSelector);
             
            }
            return enumerator.GetEnumerator();

        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (IEnumerator<TResult>)this.GetEnumerator();
        }

        #endregion

    }
    public class SelectQuery<T> : ISqoQuery<T>
    {
        IEnumerable<T> enumerator;
        public SelectQuery(IEnumerable<T> enumerator)
        {
            this.enumerator = enumerator;
        }
        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return enumerator.GetEnumerator();

        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (IEnumerator<T>)this.GetEnumerator();
        }

        #endregion






#if ASYNC
        public Task<IList<T>> ToListAsync()
        {
            throw new SiaqodbException("Does not supported,try similar Async method");
        }
#endif
    }
}
