using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
#if ASYNC_LMDB
using System.Threading.Tasks;
using Sqo.Exceptions;
#endif

namespace Sqo
{
	class ProjectionSelectReader<T,TSource> : ISqoQuery<T>
	{

		EnumeratorSelect<T> enumerator;
		SqoQuery<TSource> query;
		public ProjectionSelectReader(List<SqoColumn> columns, Func<ProjectionRow, T> projector, SqoQuery<TSource> query )
		{
			this.enumerator = new EnumeratorSelect<T>(columns, projector);
			this.query = query;
		}
#if ASYNC_LMDB
        public async Task<IList<T>> ToListAsync()
        {
            EnumeratorSelect<T> e = this.enumerator;
            e.siaqodb = query.Siaqodb;
            List<int> oids = await this.query.GetFilteredOidsAsync();

            if (oids == null)
            {
                oids = await e.siaqodb.LoadAllOIDsAsync<TSource>();

            }
            e.oids = oids;

            if (e == null)
            {
                throw new InvalidOperationException("Cannot enumerate more than once");
            }
            List<T> list = new List<T>();
            while (e.MoveNext())
            {
                list.Add(e.Current);
            }
            return list;

        }
#endif
		public IEnumerator<T> GetEnumerator()
		{
			EnumeratorSelect<T> e = this.enumerator;
			e.siaqodb = query.Siaqodb;
			List<int> oids = this.query.GetFilteredOids();

            if (oids == null)
            {
                oids = e.siaqodb.LoadAllOIDs<TSource>();

            }
			e.oids = oids;

			if (e == null)
			{
				throw new InvalidOperationException("Cannot enumerate more than once");
			}

			//this.enumerator = null;
			return e;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}





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
    class EnumeratorSelect<T> : ProjectionRow,  IEnumerator<T>, IEnumerator, IDisposable
#if ASYNC_LMDB
        ,ISqoAsyncEnumerator<T>
#endif
    {
        List<SqoColumn> columns;
        T current;
        internal Siaqodb siaqodb;
        Func<ProjectionRow, T> projector;
        internal List<int> oids;
        int currentIndex = 0;
#if ASYNC_LMDB
        int currentColumnIndex = 0;
#endif
        internal EnumeratorSelect(List<SqoColumn> columns, Func<ProjectionRow, T> projector)
        {
            this.columns = columns;
            this.projector = projector;
        }
        public override object GetValue(int index)
        {

            SqoColumn col = columns[index];
            if (col.IsFullObject)
            {
                return siaqodb.LoadObjectByOID(col.SourceType, oids[currentIndex]);
            }
            else
            {
                return siaqodb.LoadValue(oids[currentIndex], col.SourcePropName, col.SourceType);
            }



        }
#if ASYNC_LMDB
        public async Task<object> GetValueAsync(int index)
        {

            SqoColumn col = columns[index];
            if (col.IsFullObject)
            {
                return await siaqodb.LoadObjectByOIDAsync(col.SourceType, oids[currentIndex]);
            }
            else
            {
                return await siaqodb.LoadValueAsync(oids[currentIndex], col.SourcePropName, col.SourceType);
            }
        }
#endif
        public T Current
        {
            get { return this.current; }
        }
        object IEnumerator.Current
        {
            get { return this.current; }
        }

        public bool MoveNext()
        {
            //if (this.reader.Read())
            if (oids.Count > currentIndex)
            {
                this.current = this.projector(this);
                currentIndex++;
                return true;
            }
            else
            {
                this.Reset();
            }
            return false;
        }
#if ASYNC_LMDB
       
        public async Task<bool> MoveNextAsync()
        {
            throw new SiaqodbException("Not supported async operation");
        }
#endif
        public void Reset()
        {
            this.currentIndex = 0;
        }
        public void Dispose()
        {

        }

    }

}
