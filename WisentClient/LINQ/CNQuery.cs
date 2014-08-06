using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
#if ASYNC
using System.Threading.Tasks;

#endif

namespace CryptonorClient
{
    
   public class CNQuery<T>: Sqo.ISqoQuery<T> where T:Sqo.CryptonorObject
    {
        Expression expression;
        long continuationToken;
        public Expression Expression { get { return expression; } set { expression = value; } }
        IBucket bucket;
        public CNQuery(IBucket bucket)
        {
            this.bucket = bucket;
        }
        public CNQuery(IBucket bucket,long continuationToken)
        {
            this.bucket = bucket;
            this.continuationToken = continuationToken;
        }
        private IList<T> oList;
        
#if ASYNC
       
        public async Task<IList<T>> ToListAsync()
        {
            if (oList == null)
            {
                if (expression == null)
                {
                    var result =await bucket.GetAll();
                    oList = (IList<T>)result.Objects;

                }
                else
                {
                    var result =await bucket.Get(this.expression,this.continuationToken);
                    oList = (IList<T>)result.Objects ;

                }
            }
            return oList;
        }
        public async Task<CryptonorResultSet> GetResultSetAsync<T>()
        {

            if (expression == null)
            {
                return await bucket.GetAll();

            }
            else
            {

                return await bucket.Get(this.expression,this.continuationToken);

            }
            
            
           
        }
#endif

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
           
           
                if (oList == null)
                {
                    if (expression == null)
                    {
                        //oList = (IList<T>)bucket.GetAll();
                        throw new Exception("TODO");
                    }
                    else
                    {
                        throw new Exception("TODO");
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

     

        #region Sqo.ISqoQuery<T> Members

        public Sqo.ISqoQuery<T> SqoWhere(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.Where(this, expression);
        }

        public Sqo.ISqoQuery<TRet> SqoSelect<TRet>(Expression<Func<T, TRet>> selector)
        {
            return CNQueryExtensionsImpl.Select(this, selector);
        }

        public Sqo.ISqoQuery<TResult> SqoJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Expression<Func<T, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<T, TInner, TResult>> resultSelector)
        {
            return CNQueryExtensionsImpl.Join(this, inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public int SqoCount()
        {
            return CNQueryExtensionsImpl.Count(this);
        }
#if ASYNC
        public Task<int> SqoCountAsync()
        {
            return CNQueryExtensionsImpl.CountAsync(this);
        }
#endif
        public int SqoCount(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.Count(this, expression);
        }
#if ASYNC
        public Task<int> SqoCountAsync(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.CountAsync(this, expression);
        }
#endif
        public T SqoFirstOrDefault()
        {
            return CNQueryExtensionsImpl.FirstOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoFirstOrDefaultAsync()
        {
            return CNQueryExtensionsImpl.FirstOrDefaultAsync(this);
        }
#endif
        public T SqoFirstOrDefault(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.FirstOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoFirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.FirstOrDefaultAsync(this, expression);
        }
#endif
        public T SqoFirst()
        {
            return CNQueryExtensionsImpl.First(this);
        }
#if ASYNC
        public Task<T> SqoFirstAsync()
        {
            return CNQueryExtensionsImpl.FirstAsync(this);
        }
#endif
        public T SqoFirst(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.First(this, expression);
        }
#if ASYNC
        public Task<T> SqoFirstAsync(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.FirstAsync(this, expression);
        }
#endif
        public bool SqoAny()
        {
            return CNQueryExtensionsImpl.Any(this);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync()
        {
            return CNQueryExtensionsImpl.AnyAsync(this);
        }
#endif
        public bool SqoAny(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.Any(this, expression);
        }
#if ASYNC
        public Task<bool> SqoAnyAsync(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.AnyAsync(this, expression);
        }
#endif
        public T SqoLast()
        {
            return CNQueryExtensionsImpl.Last(this);
        }
#if ASYNC
        public Task<T> SqoLastAsync()
        {
            return CNQueryExtensionsImpl.LastAsync(this);
        }
#endif
        public T SqoLast(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.Last(this, expression);
        }
#if ASYNC
        public Task<T> SqoLastAsync(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.LastAsync(this, expression);
        }
#endif
        public T SqoLastOrDefault()
        {
            return CNQueryExtensionsImpl.LastOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoLastOrDefaultAsync()
        {
            return CNQueryExtensionsImpl.LastOrDefaultAsync(this);
        }
#endif
        public T SqoLastOrDefault(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.LastOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoLastOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.LastOrDefaultAsync(this, expression);
        }
#endif
        public T SqoSingle()
        {
            return CNQueryExtensionsImpl.Single(this);
        }
#if ASYNC
        public Task<T> SqoSingleAsync()
        {
            return CNQueryExtensionsImpl.SingleAsync(this);
        }
#endif
        public T SqoSingle(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.Single(this, expression);
        }
#if ASYNC
        public Task<T> SqoSingleAsync(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.SingleAsync(this, expression);
        }
#endif
        public T SqoSingleOrDefault()
        {
            return CNQueryExtensionsImpl.SingleOrDefault(this);
        }
#if ASYNC
        public Task<T> SqoSingleOrDefaultAsync()
        {
            return CNQueryExtensionsImpl.SingleOrDefaultAsync(this);
        }
#endif
        public T SqoSingleOrDefault(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.SingleOrDefault(this, expression);
        }
#if ASYNC
        public Task<T> SqoSingleOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return CNQueryExtensionsImpl.SingleOrDefaultAsync(this, expression);
        }
#endif
        public Sqo.ISqoQuery<T> SqoTake(int count)
        {
            return CNQueryExtensionsImpl.Take(this, count);
        }
#if ASYNC
        public Task<Sqo.ISqoQuery<T>> SqoTakeAsync(int count)
        {
            return CNQueryExtensionsImpl.TakeAsync(this, count);
        }
#endif
        public Sqo.ISqoQuery<T> SqoSkip(int count)
        {
            return CNQueryExtensionsImpl.Skip(this, count);
        }
#if ASYNC
        public Task<Sqo.ISqoQuery<T>> SqoSkipAsync(int count)
        {
            return CNQueryExtensionsImpl.SkipAsync(this, count);
        }
#endif
        public Sqo.ISqoQuery<T> SqoInclude(string path)
        {
            throw new NotImplementedException();
        }

		
        #endregion


        public Sqo.ISqoOrderedQuery<T> SqoOrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public Sqo.ISqoOrderedQuery<T> SqoOrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public Sqo.ISqoOrderedQuery<T> SqoThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public Sqo.ISqoOrderedQuery<T> SqoThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }
    }
    
}
