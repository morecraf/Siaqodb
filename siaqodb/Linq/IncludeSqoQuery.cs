using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo
{
    #if KEVAST
    internal
#else
        public
#endif
        class IncludeSqoQuery<T> : ISqoQuery<T>
    {
        internal List<string> includes=new List<string>();
        SqoQuery<T> originalQuery;

#if ASYNC
            ISqoAsyncEnumerator<T> enumerator;
#else
            IEnumerator<T> enumerator;
#endif
            public IncludeSqoQuery(SqoQuery<T> query, params string[] properties)
        {
            this.originalQuery = query;
            includes.AddRange(properties);
        }
#if ASYNC
        public async Task<IList<T>> ToListAsync()
        {
            IObjectList<T> list = new ObjectList<T>();
            ISqoAsyncEnumerator<T> asyncEnum = await this.GetEnumeratorAsync();
            while (await asyncEnum.MoveNextAsync())
            {
                list.Add(asyncEnum.Current);
            }
            return list;

        }
        public async Task<ISqoAsyncEnumerator<T>> GetEnumeratorAsync()
        {
            if (this.enumerator == null)
            {
                List<int> oids = await originalQuery.GetOidsAsync();
                this.enumerator = new LazyEnumerator<T>(originalQuery.Siaqodb, oids, includes);
            }
            return this.enumerator;
        }
#endif
        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (this.enumerator == null)
            {
                List<int> oids=originalQuery.GetOids();
                this.enumerator = new LazyEnumerator<T>(originalQuery.Siaqodb, oids,includes);
            }
            return this.enumerator;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.enumerator;
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
            return SqoQueryExtensionsImpl.Join(this, inner,outerKeySelector,innerKeySelector,resultSelector);
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
            return SqoQueryExtensionsImpl.Count(this,expression);
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
            return SqoQueryExtensionsImpl.First(this,expression);
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
            return SqoQueryExtensionsImpl.Any(this,expression);
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
            return SqoQueryExtensionsImpl.Include(this,path);
        }

		#if !UNITY3D  || XIOS
        public ISqoOrderedQuery<T> SqoOrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return SqoQueryExtensionsImpl.OrderBy(this,keySelector);
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
