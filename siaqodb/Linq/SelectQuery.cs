using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
using Sqo.Exceptions;
using System.Linq.Expressions;
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


        #region ISqoQuery<T> Members

        public ISqoQuery<T> SqoWhere(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Where(this, expression);
        }

        public ISqoQuery<TRet> SqoSelect<TRet>(Expression<Func<T, TRet>> selector)
        {
            return SqoQueryExtensionsImpl.Select(this, selector);
        }

        public ISqoQuery<TResult> SqoJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Expression<Func<T, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<T, TInner, TResult>> resultSelector)
        {
            return SqoQueryExtensionsImpl.Join(this, inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public int SqoCount()
        {
            return SqoQueryExtensionsImpl.Count(this);
        }
#if ASYNC
        public Task<int> SqoCountAsync()
        {
            return SqoQueryExtensionsImpl.CountAsync(this);
        }
#endif
        public int SqoCount(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Count(this, expression);
        }
#if ASYNC
        public Task<int> SqoCountAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.CountAsync(this, expression);
        }
#endif
        public T SqoFirstOrDefault()
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoFirstOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this);
        }
#endif
        public T SqoFirstOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoFirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this, expression);
        }
#endif
        public T SqoFirst()
        {
            return SqoQueryExtensionsImpl.First(this);
        }
#if ASYNC
        public Task<T> SqoFirstAsync()
        {
            return SqoQueryExtensionsImpl.FirstAsync(this);
        }
#endif
        public T SqoFirst(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.First(this, expression);
        }
#if ASYNC
        public Task<T> SqoFirstAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstAsync(this, expression);
        }
#endif
        public bool SqoAny()
        {
            return SqoQueryExtensionsImpl.Any(this);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync()
        {
            return SqoQueryExtensionsImpl.AnyAsync(this);
        }
#endif
        public bool SqoAny(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Any(this, expression);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.AnyAsync(this, expression);
        }
#endif
        public T SqoLast()
        {
            return SqoQueryExtensionsImpl.Last(this);
        }
#if ASYNC
        public Task<T> SqoLastAsync()
        {
            return SqoQueryExtensionsImpl.LastAsync(this);
        }
#endif
        public T SqoLast(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Last(this, expression);
        }
#if ASYNC
        public Task<T> SqoLastAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastAsync(this, expression);
        }
#endif
        public T SqoLastOrDefault()
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoLastOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this);
        }
#endif
        public T SqoLastOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoLastOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this, expression);
        }
#endif
        public T SqoSingle()
        {
            return SqoQueryExtensionsImpl.Single(this);
        }
#if ASYNC
        public Task<T> SqoSingleAsync()
        {
            return SqoQueryExtensionsImpl.SingleAsync(this);
        }
#endif
        public T SqoSingle(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Single(this, expression);
        }
#if ASYNC
        public Task<T> SqoSingleAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleAsync(this, expression);
        }
#endif
        public T SqoSingleOrDefault()
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoSingleOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this);
        }
#endif
        public T SqoSingleOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoSingleOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this, expression);
        }
#endif
        public ISqoQuery<T> SqoTake(int count)
        {
            return SqoQueryExtensionsImpl.Take(this, count);
        }
#if ASYNC
        public Task<ISqoQuery<T>> SqoTakeAsync(int count)
        {
            return SqoQueryExtensionsImpl.TakeAsync(this, count);
        }
#endif
        public ISqoQuery<T> SqoSkip(int count)
        {
            return SqoQueryExtensionsImpl.Skip(this, count);
        }
#if ASYNC
        public Task<ISqoQuery<T>> SqoSkipAsync(int count)
        {
            return SqoQueryExtensionsImpl.SkipAsync(this, count);
        }
#endif
        public ISqoQuery<T> SqoInclude(string path)
        {
            return SqoQueryExtensionsImpl.Include(this, path);
        }

#if !UNITY3D
        public ISqoOrderedQuery<T> SqoOrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderBy(this, keySelector);
        }

        public ISqoOrderedQuery<T> SqoOrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderByDescending(this, keySelector);
        }

        public ISqoOrderedQuery<T> SqoThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.ThenBy(this as ISqoOrderedQuery<T>, keySelector);
        }

        public ISqoOrderedQuery<T> SqoThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.ThenByDescending(this as ISqoOrderedQuery<T>, keySelector);
        }
#endif
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


        #region ISqoQuery<V> Members

        public ISqoQuery<V> SqoWhere(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Where(this, expression);
        }

        public ISqoQuery<TRet> SqoSelect<TRet>(Expression<Func<V, TRet>> selector)
        {
            return SqoQueryExtensionsImpl.Select(this, selector);
        }

        public ISqoQuery<TResult> SqoJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Expression<Func<V, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<V, TInner, TResult>> resultSelector)
        {
            return SqoQueryExtensionsImpl.Join(this, inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public int SqoCount()
        {
            return SqoQueryExtensionsImpl.Count(this);
        }
#if ASYNC
        public Task<int> SqoCountAsync()
        {
            return SqoQueryExtensionsImpl.CountAsync(this);
        }
#endif
        public int SqoCount(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Count(this, expression);
        }
#if ASYNC
        public Task<int> SqoCountAsync(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.CountAsync(this, expression);
        }
#endif
        public V SqoFirstOrDefault()
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this);
        }
#if ASYNC
        public Task<V> SqoFirstOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this);
        }
#endif
        public V SqoFirstOrDefault(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this, expression);
        }
#if ASYNC
        public Task<V> SqoFirstOrDefaultAsync(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this, expression);
        }
#endif
        public V SqoFirst()
        {
            return SqoQueryExtensionsImpl.First(this);
        }
#if ASYNC
        public Task<V> SqoFirstAsync()
        {
            return SqoQueryExtensionsImpl.FirstAsync(this);
        }
#endif
        public V SqoFirst(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.First(this, expression);
        }
#if ASYNC
        public Task<V> SqoFirstAsync(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstAsync(this, expression);
        }
#endif
        public bool SqoAny()
        {
            return SqoQueryExtensionsImpl.Any(this);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync()
        {
            return SqoQueryExtensionsImpl.AnyAsync(this);
        }
#endif
        public bool SqoAny(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Any(this, expression);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.AnyAsync(this, expression);
        }
#endif
        public V SqoLast()
        {
            return SqoQueryExtensionsImpl.Last(this);
        }
#if ASYNC
        public Task<V> SqoLastAsync()
        {
            return SqoQueryExtensionsImpl.LastAsync(this);
        }
#endif
        public V SqoLast(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Last(this, expression);
        }
#if ASYNC
        public Task<V> SqoLastAsync(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastAsync(this, expression);
        }
#endif
        public V SqoLastOrDefault()
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this);
        }
#if ASYNC
        public Task<V> SqoLastOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this);
        }
#endif
        public V SqoLastOrDefault(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this, expression);
        }
#if ASYNC
        public Task<V> SqoLastOrDefaultAsync(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this, expression);
        }
#endif
        public V SqoSingle()
        {
            return SqoQueryExtensionsImpl.Single(this);
        }
#if ASYNC
        public Task<V> SqoSingleAsync()
        {
            return SqoQueryExtensionsImpl.SingleAsync(this);
        }
#endif
        public V SqoSingle(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Single(this, expression);
        }
#if ASYNC
        public Task<V> SqoSingleAsync(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleAsync(this, expression);
        }
#endif
        public V SqoSingleOrDefault()
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this);
        }
#if ASYNC
        public Task<V> SqoSingleOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this);
        }
#endif
        public V SqoSingleOrDefault(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this, expression);
        }
#if ASYNC
        public Task<V> SqoSingleOrDefaultAsync(Expression<Func<V, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this, expression);
        }
#endif
        public ISqoQuery<V> SqoTake(int count)
        {
            return SqoQueryExtensionsImpl.Take(this, count);
        }
#if ASYNC
        public Task<ISqoQuery<V>> SqoTakeAsync(int count)
        {
            return SqoQueryExtensionsImpl.TakeAsync(this, count);
        }
#endif
        public ISqoQuery<V> SqoSkip(int count)
        {
            return SqoQueryExtensionsImpl.Skip(this, count);
        }
#if ASYNC
        public Task<ISqoQuery<V>> SqoSkipAsync(int count)
        {
            return SqoQueryExtensionsImpl.SkipAsync(this, count);
        }
#endif
        public ISqoQuery<V> SqoInclude(string path)
        {
            return SqoQueryExtensionsImpl.Include(this, path);
        }

#if !UNITY3D
        public ISqoOrderedQuery<V> SqoOrderBy<TKey>(Expression<Func<V, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderBy(this, keySelector);
        }

        public ISqoOrderedQuery<V> SqoOrderByDescending<TKey>(Expression<Func<V, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderByDescending(this, keySelector);
        }

        public ISqoOrderedQuery<V> SqoThenBy<TKey>(Expression<Func<V, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.ThenBy(this as ISqoOrderedQuery<V>, keySelector);
        }

        public ISqoOrderedQuery<V> SqoThenByDescending<TKey>(Expression<Func<V, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.ThenByDescending(this as ISqoOrderedQuery<V>, keySelector);
        }
#endif
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


        #region ISqoQuery<TResult> Members

        public ISqoQuery<TResult> SqoWhere(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Where(this, expression);
        }

        public ISqoQuery<TRet> SqoSelect<TRet>(Expression<Func<TResult, TRet>> selector)
        {
            return SqoQueryExtensionsImpl.Select(this, selector);
        }
       
        public int SqoCount()
        {
            return SqoQueryExtensionsImpl.Count(this);
        }
#if ASYNC
        public Task<int> SqoCountAsync()
        {
            return SqoQueryExtensionsImpl.CountAsync(this);
        }
#endif
        public int SqoCount(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Count(this, expression);
        }
#if ASYNC
        public Task<int> SqoCountAsync(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.CountAsync(this, expression);
        }
#endif
        public TResult SqoFirstOrDefault()
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this);
        }
#if ASYNC
        public Task<TResult> SqoFirstOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this);
        }
#endif
        public TResult SqoFirstOrDefault(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this, expression);
        }
#if ASYNC
        public Task<TResult> SqoFirstOrDefaultAsync(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this, expression);
        }
#endif
        public TResult SqoFirst()
        {
            return SqoQueryExtensionsImpl.First(this);
        }
#if ASYNC
        public Task<TResult> SqoFirstAsync()
        {
            return SqoQueryExtensionsImpl.FirstAsync(this);
        }
#endif
        public TResult SqoFirst(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.First(this, expression);
        }
#if ASYNC
        public Task<TResult> SqoFirstAsync(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstAsync(this, expression);
        }
#endif
        public bool SqoAny()
        {
            return SqoQueryExtensionsImpl.Any(this);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync()
        {
            return SqoQueryExtensionsImpl.AnyAsync(this);
        }
#endif
        public bool SqoAny(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Any(this, expression);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.AnyAsync(this, expression);
        }
#endif
        public TResult SqoLast()
        {
            return SqoQueryExtensionsImpl.Last(this);
        }
#if ASYNC
        public Task<TResult> SqoLastAsync()
        {
            return SqoQueryExtensionsImpl.LastAsync(this);
        }
#endif
        public TResult SqoLast(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Last(this, expression);
        }
#if ASYNC
        public Task<TResult> SqoLastAsync(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastAsync(this, expression);
        }
#endif
        public TResult SqoLastOrDefault()
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this);
        }
#if ASYNC
        public Task<TResult> SqoLastOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this);
        }
#endif
        public TResult SqoLastOrDefault(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this, expression);
        }
#if ASYNC
        public Task<TResult> SqoLastOrDefaultAsync(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this, expression);
        }
#endif
        public TResult SqoSingle()
        {
            return SqoQueryExtensionsImpl.Single(this);
        }
#if ASYNC
        public Task<TResult> SqoSingleAsync()
        {
            return SqoQueryExtensionsImpl.SingleAsync(this);
        }
#endif
        public TResult SqoSingle(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Single(this, expression);
        }
#if ASYNC
        public Task<TResult> SqoSingleAsync(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleAsync(this, expression);
        }
#endif
        public TResult SqoSingleOrDefault()
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this);
        }
#if ASYNC
        public Task<TResult> SqoSingleOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this);
        }
#endif
        public TResult SqoSingleOrDefault(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this, expression);
        }
#if ASYNC
        public Task<TResult> SqoSingleOrDefaultAsync(Expression<Func<TResult, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this, expression);
        }
#endif
        public ISqoQuery<TResult> SqoTake(int count)
        {
            return SqoQueryExtensionsImpl.Take(this, count);
        }
#if ASYNC
        public Task<ISqoQuery<TResult>> SqoTakeAsync(int count)
        {
            return SqoQueryExtensionsImpl.TakeAsync(this, count);
        }
#endif
        public ISqoQuery<TResult> SqoSkip(int count)
        {
            return SqoQueryExtensionsImpl.Skip(this, count);
        }
#if ASYNC
        public Task<ISqoQuery<TResult>> SqoSkipAsync(int count)
        {
            return SqoQueryExtensionsImpl.SkipAsync(this, count);
        }
#endif
        public ISqoQuery<TResult> SqoInclude(string path)
        {
            return SqoQueryExtensionsImpl.Include(this, path);
        }

#if !UNITY3D
        public ISqoOrderedQuery<TResult> SqoOrderBy<TKeyO>(Expression<Func<TResult, TKeyO>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderBy(this, keySelector);
        }

        public ISqoOrderedQuery<TResult> SqoOrderByDescending<TKeyO>(Expression<Func<TResult, TKeyO>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderByDescending(this, keySelector);
        }

        public ISqoOrderedQuery<TResult> SqoThenBy<TKeyO>(Expression<Func<TResult, TKeyO>> keySelector)
        {
            return SqoQueryExtensionsImpl.ThenBy(this as ISqoOrderedQuery<TResult>, keySelector);
        }

        public ISqoOrderedQuery<TResult> SqoThenByDescending<TKeyO>(Expression<Func<TResult, TKeyO>> keySelector)
        {
            return SqoQueryExtensionsImpl.ThenByDescending(this as ISqoOrderedQuery<TResult>, keySelector);
        }
#endif
        #endregion






        #region IEnumerable<TResult> Members

        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISqoQuery<TResult> Members


        public ISqoQuery<TResult2> SqoJoin<TInner1, TKey1,TResult2>(IEnumerable<TInner1> inner, Expression<Func<TResult, TKey1>> outerKeySelector, Expression<Func<TInner1, TKey1>> innerKeySelector, Expression<Func<TResult, TInner1, TResult2>> resultSelector)
        {
            return SqoQueryExtensionsImpl.Join(this, inner, outerKeySelector, innerKeySelector, resultSelector);
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

        #region ISqoQuery<T> Members

        public ISqoQuery<T> SqoWhere(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Where(this, expression);
        }

        public ISqoQuery<TRet> SqoSelect<TRet>(Expression<Func<T, TRet>> selector)
        {
            return SqoQueryExtensionsImpl.Select(this, selector);
        }

        public ISqoQuery<TResult> SqoJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Expression<Func<T, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<T, TInner, TResult>> resultSelector)
        {
            return SqoQueryExtensionsImpl.Join(this, inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public int SqoCount()
        {
            return SqoQueryExtensionsImpl.Count(this);
        }
#if ASYNC
        public Task<int> SqoCountAsync()
        {
            return SqoQueryExtensionsImpl.CountAsync(this);
        }
#endif
        public int SqoCount(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Count(this, expression);
        }
#if ASYNC
        public Task<int> SqoCountAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.CountAsync(this, expression);
        }
#endif
        public T SqoFirstOrDefault()
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoFirstOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this);
        }
#endif
        public T SqoFirstOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoFirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this, expression);
        }
#endif
        public T SqoFirst()
        {
            return SqoQueryExtensionsImpl.First(this);
        }
#if ASYNC
        public Task<T> SqoFirstAsync()
        {
            return SqoQueryExtensionsImpl.FirstAsync(this);
        }
#endif
        public T SqoFirst(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.First(this, expression);
        }
#if ASYNC
        public Task<T> SqoFirstAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstAsync(this, expression);
        }
#endif
        public bool SqoAny()
        {
            return SqoQueryExtensionsImpl.Any(this);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync()
        {
            return SqoQueryExtensionsImpl.AnyAsync(this);
        }
#endif
        public bool SqoAny(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Any(this, expression);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.AnyAsync(this, expression);
        }
#endif
        public T SqoLast()
        {
            return SqoQueryExtensionsImpl.Last(this);
        }
#if ASYNC
        public Task<T> SqoLastAsync()
        {
            return SqoQueryExtensionsImpl.LastAsync(this);
        }
#endif
        public T SqoLast(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Last(this, expression);
        }
#if ASYNC
        public Task<T> SqoLastAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastAsync(this, expression);
        }
#endif
        public T SqoLastOrDefault()
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoLastOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this);
        }
#endif
        public T SqoLastOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoLastOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this, expression);
        }
#endif
        public T SqoSingle()
        {
            return SqoQueryExtensionsImpl.Single(this);
        }
#if ASYNC
        public Task<T> SqoSingleAsync()
        {
            return SqoQueryExtensionsImpl.SingleAsync(this);
        }
#endif
        public T SqoSingle(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Single(this, expression);
        }
#if ASYNC
        public Task<T> SqoSingleAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleAsync(this, expression);
        }
#endif
        public T SqoSingleOrDefault()
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoSingleOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this);
        }
#endif
        public T SqoSingleOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoSingleOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this, expression);
        }
#endif
        public ISqoQuery<T> SqoTake(int count)
        {
            return SqoQueryExtensionsImpl.Take(this, count);
        }
#if ASYNC
        public Task<ISqoQuery<T>> SqoTakeAsync(int count)
        {
            return SqoQueryExtensionsImpl.TakeAsync(this, count);
        }
#endif
        public ISqoQuery<T> SqoSkip(int count)
        {
            return SqoQueryExtensionsImpl.Skip(this, count);
        }
#if ASYNC
        public Task<ISqoQuery<T>> SqoSkipAsync(int count)
        {
            return SqoQueryExtensionsImpl.SkipAsync(this, count);
        }
#endif
        public ISqoQuery<T> SqoInclude(string path)
        {
            return SqoQueryExtensionsImpl.Include(this, path);
        }

#if !UNITY3D
        public ISqoOrderedQuery<T> SqoOrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderBy(this, keySelector);
        }

        public ISqoOrderedQuery<T> SqoOrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderByDescending(this, keySelector);
        }

        public ISqoOrderedQuery<T> SqoThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.ThenBy(this as ISqoOrderedQuery<T>, keySelector);
        }

        public ISqoOrderedQuery<T> SqoThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.ThenByDescending(this as ISqoOrderedQuery<T>, keySelector);
        }
#endif
        #endregion
    }
}
