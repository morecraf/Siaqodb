using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo;
using System.Linq.Expressions;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Sqo
{
    public interface ISqoQuery<T>:IEnumerable<T>
    {
        ISqoQuery<T> SqoWhere(Expression<Func<T, bool>> expression);
        ISqoQuery<TRet> SqoSelect<TRet>(Expression<Func<T, TRet>> selector);
        ISqoQuery<TResult> SqoJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Expression<Func<T, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<T, TInner, TResult>> resultSelector);
        int SqoCount();
#if ASYNC
       Task<int> SqoCountAsync();
#endif
        int SqoCount(Expression<Func<T, bool>> expression);
		#if ASYNC
       Task<int> SqoCountAsync(Expression<Func<T, bool>> expression);
#endif
        T SqoFirstOrDefault();
#if ASYNC
       Task<T> SqoFirstOrDefaultAsync();
#endif
       T SqoFirstOrDefault(Expression<Func<T, bool>> expression);

#if ASYNC
       Task<T> SqoFirstOrDefaultAsync(Expression<Func<T, bool>> expression);   
#endif
       T SqoFirst();
#if ASYNC
       Task<T> SqoFirstAsync();
#endif
       T SqoFirst(Expression<Func<T, bool>> expression);

#if ASYNC
       Task<T> SqoFirstAsync(Expression<Func<T, bool>> expression);
#endif
       bool SqoAny();
#if ASYNC
       Task<bool> SqoAnyAsync();
#endif
       bool SqoAny(Expression<Func<T, bool>> expression);
#if ASYNC
       Task<bool> SqoAnyAsync(Expression<Func<T, bool>> expression);
#endif
       T SqoLast();
#if ASYNC
       Task<T> SqoLastAsync();
#endif
       T SqoLast(Expression<Func<T, bool>> expression);
#if ASYNC
       Task<T> SqoLastAsync(Expression<Func<T, bool>> expression);
#endif
       T SqoLastOrDefault();
#if ASYNC
       Task<T> SqoLastOrDefaultAsync();
#endif
       T SqoLastOrDefault(Expression<Func<T, bool>> expression);
#if ASYNC
       Task<T> SqoLastOrDefaultAsync(Expression<Func<T, bool>> expression);
#endif
       T SqoSingle();
#if ASYNC
       Task<T> SqoSingleAsync();
#endif
       T SqoSingle(Expression<Func<T, bool>> expression);
#if ASYNC
       Task<T> SqoSingleAsync(Expression<Func<T, bool>> expression);
#endif
       T SqoSingleOrDefault();
#if ASYNC
       Task<T> SqoSingleOrDefaultAsync();
#endif
       T SqoSingleOrDefault(Expression<Func<T, bool>> expression);
#if ASYNC
       Task<T> SqoSingleOrDefaultAsync(Expression<Func<T, bool>> expression);
#endif
       ISqoQuery<T> SqoTake(int count);
#if ASYNC
       Task<ISqoQuery<T>> SqoTakeAsync(int count);
#endif
       ISqoQuery<T> SqoSkip(int count);
#if ASYNC
       Task<ISqoQuery<T>> SqoSkipAsync(int count);
#endif
       ISqoQuery<T> SqoInclude(string path);

		#if !UNITY3D || XIOS
       ISqoOrderedQuery<T> SqoOrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
       ISqoOrderedQuery<T> SqoOrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
       ISqoOrderedQuery<T> SqoThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
       ISqoOrderedQuery<T> SqoThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
#endif


#if ASYNC
        Task<IList<T>> ToListAsync();
#endif
    }
    public interface ISqoOrderedQuery<T> : ISqoQuery<T>,IOrderedEnumerable<T>
    { 
#if ASYNC
       
#endif
    }
}
