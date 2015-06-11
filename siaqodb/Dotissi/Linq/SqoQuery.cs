using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Expressions;
using Sqo;
#if ASYNC_LMDB
using System.Threading.Tasks;
#endif

namespace Dotissi
{
    #if KEVAST
    internal
#else
        public
#endif
        class SqoQuery<T>: ISqoQuery<T>
    {
        Siaqodb siaqodb;
        Expression expression;
        public Expression Expression { get { return expression; } set { expression = value; } }
        internal SqoQuery(Siaqodb siaqodb)
        {
            this.siaqodb = siaqodb;
            
        }
        internal Siaqodb Siaqodb { get { return siaqodb; } }
        private IObjectList<T> oList;
        private List<int> oidsList;
        public List<int> GetFilteredOids()
        {
            if (expression == null)
                return null;
            else
            {
                return siaqodb.LoadOids<T>(this.expression);
            }
        }
#if ASYNC_LMDB
        public async Task<List<int>> GetFilteredOidsAsync()
        {
            if (expression == null)
                return null;
            else
            {
                return await siaqodb.LoadOidsAsync<T>(this.expression);
            }
        }
#endif
        public int CountOids()
        {
            if (expression == null)
            {
                return siaqodb.Count<T>();
            }
            else
            {
                return siaqodb.LoadOids<T>(this.expression).Count;
            }
        }
#if ASYNC_LMDB
        public async Task<int> CountOidsAsync()
        {
            if (expression == null)
            {
                return await siaqodb.CountAsync<T>();
            }
            else
            {
                List<int> list = await siaqodb.LoadOidsAsync<T>(this.expression);
                return list.Count;
            }
        }
        public async Task<IList<T>> ToListAsync()
        {
            if (oList == null)
            {
                if (expression == null)
                {
                    oList = await siaqodb.LoadAllAsync<T>();

                }
                else
                {
                    oList = await siaqodb.LoadAsync<T>(this.expression);

                }
            }
            return oList;
        }
#endif

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
           
           
                if (oList == null)
                {
                    if (expression == null)
                    {
                        oList = siaqodb.LoadAll<T>();
                        
                    }
                    else
                    {
                        oList = siaqodb.Load<T>(this.expression);
                    }
                }
            
            return oList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
			return (IEnumerator < T > )this.GetEnumerator();
        }

        #endregion

        public LazyEnumerator<T> GetLazyEnumerator()
        {
            if (oidsList == null)
            {
                if (expression == null)
                {
                    oidsList = siaqodb.LoadAllOIDs<T>();

                }
                else
                {
                    oidsList = siaqodb.LoadOids<T>(this.expression);
                }
            }
            return new LazyEnumerator<T>(this.siaqodb, oidsList);
            
        }
#if ASYNC_LMDB
        public async Task<LazyEnumerator<T>> GetLazyEnumeratorAsync()
        {
            if (oidsList == null)
            {
                if (expression == null)
                {
                    oidsList = await siaqodb.LoadAllOIDsAsync<T>();

                }
                else
                {
                    oidsList = await siaqodb.LoadOidsAsync<T>(this.expression);
                }
            }
            return new LazyEnumerator<T>(this.siaqodb, oidsList);

        }
#endif
        public T GetLast(bool throwExce)
        {
            if (oidsList == null)
            {
                if (expression == null)
                {
                    oidsList = siaqodb.LoadAllOIDs<T>();

                }
                else
                {
                    oidsList = siaqodb.LoadOids<T>(this.expression);
                }
            }
            if (oidsList.Count > 0)
            {
                return siaqodb.LoadObjectByOID<T>(oidsList[oidsList.Count - 1]);
            }
            else
            {
                if (throwExce)
                {
                    throw new InvalidOperationException("no match found");
                }
                else
                {
                    return default(T);
                }
            }
        }
#if ASYNC_LMDB
        public async Task<T> GetLastAsync(bool throwExce)
        {
            if (oidsList == null)
            {
                if (expression == null)
                {
                    oidsList = await siaqodb.LoadAllOIDsAsync<T>();

                }
                else
                {
                    oidsList = await siaqodb.LoadOidsAsync<T>(this.expression);
                }
            }
            if (oidsList.Count > 0)
            {
                return await siaqodb.LoadObjectByOIDAsync<T>(oidsList[oidsList.Count - 1]);
            }
            else
            {
                if (throwExce)
                {
                    throw new InvalidOperationException("no match found");
                }
                else
                {
                    return default(T);
                }
            }
        }
#endif
        public List<int> GetOids()
        {
            if (expression == null)
                return siaqodb.LoadAllOIDs<T>();
            else
            {
                return siaqodb.LoadOids<T>(this.expression);
            }
        }
#if ASYNC_LMDB
        public async Task<List<int>> GetOidsAsync()
        {
            if (expression == null)
                return await siaqodb.LoadAllOIDsAsync<T>();
            else
            {
                return await siaqodb.LoadOidsAsync<T>(this.expression);
            }
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
#if ASYNC_LMDB
        public Task<int> SqoCountAsync()
        {
            return SqoQueryExtensionsImpl.CountAsync(this);
        }
#endif
        public int SqoCount(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Count(this, expression);
        }
#if ASYNC_LMDB
        public Task<int> SqoCountAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.CountAsync(this, expression);
        }
#endif
        public T SqoFirstOrDefault()
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this);
        }
#if ASYNC_LMDB
        public Task<T> SqoFirstOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this);
        }
#endif
        public T SqoFirstOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefault(this, expression);
        }
#if ASYNC_LMDB
        public Task<T> SqoFirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstOrDefaultAsync(this, expression);
        }
#endif
        public T SqoFirst()
        {
            return SqoQueryExtensionsImpl.First(this);
        }
#if ASYNC_LMDB
        public Task<T> SqoFirstAsync()
        {
            return SqoQueryExtensionsImpl.FirstAsync(this);
        }
#endif
        public T SqoFirst(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.First(this, expression);
        }
#if ASYNC_LMDB
        public Task<T> SqoFirstAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.FirstAsync(this, expression);
        }
#endif
        public bool SqoAny()
        {
            return SqoQueryExtensionsImpl.Any(this);
        }
#if ASYNC_LMDB
        public Task<bool> SqoAnyAsync()
        {
            return SqoQueryExtensionsImpl.AnyAsync(this);
        }
#endif
        public bool SqoAny(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Any(this, expression);
        }
#if ASYNC_LMDB
        public Task<bool> SqoAnyAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.AnyAsync(this, expression);
        }
#endif
        public T SqoLast()
        {
            return SqoQueryExtensionsImpl.Last(this);
        }
#if ASYNC_LMDB
        public Task<T> SqoLastAsync()
        {
            return SqoQueryExtensionsImpl.LastAsync(this);
        }
#endif
        public T SqoLast(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Last(this, expression);
        }
#if ASYNC_LMDB
        public Task<T> SqoLastAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastAsync(this, expression);
        }
#endif
        public T SqoLastOrDefault()
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this);
        }
#if ASYNC_LMDB
        public Task<T> SqoLastOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this);
        }
#endif
        public T SqoLastOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefault(this, expression);
        }
#if ASYNC_LMDB
        public Task<T> SqoLastOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.LastOrDefaultAsync(this, expression);
        }
#endif
        public T SqoSingle()
        {
            return SqoQueryExtensionsImpl.Single(this);
        }
#if ASYNC_LMDB
        public Task<T> SqoSingleAsync()
        {
            return SqoQueryExtensionsImpl.SingleAsync(this);
        }
#endif
        public T SqoSingle(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.Single(this, expression);
        }
#if ASYNC_LMDB
        public Task<T> SqoSingleAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleAsync(this, expression);
        }
#endif
        public T SqoSingleOrDefault()
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this);
        }
#if ASYNC_LMDB
        public Task<T> SqoSingleOrDefaultAsync()
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this);
        }
#endif
        public T SqoSingleOrDefault(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefault(this, expression);
        }
#if ASYNC_LMDB
        public Task<T> SqoSingleOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return SqoQueryExtensionsImpl.SingleOrDefaultAsync(this, expression);
        }
#endif
        public ISqoQuery<T> SqoTake(int count)
        {
            return SqoQueryExtensionsImpl.Take(this, count);
        }
#if ASYNC_LMDB
        public Task<ISqoQuery<T>> SqoTakeAsync(int count)
        {
            return SqoQueryExtensionsImpl.TakeAsync(this, count);
        }
#endif
        public ISqoQuery<T> SqoSkip(int count)
        {
            return SqoQueryExtensionsImpl.Skip(this, count);
        }
#if ASYNC_LMDB
        public Task<ISqoQuery<T>> SqoSkipAsync(int count)
        {
            return SqoQueryExtensionsImpl.SkipAsync(this, count);
        }
#endif
        public ISqoQuery<T> SqoInclude(string path)
        {
            return SqoQueryExtensionsImpl.Include(this, path);
        }

		#if !UNITY3D  || XIOS
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
