using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo
{
    
    public static class SqoQueryExtensions
    {

        public static ISqoQuery<TSource> Where<TSource>(this ISqoQuery<TSource> self, Expression<Func<TSource, bool>> expression)
        {
            return self.SqoWhere(expression);
        }
      
		public static ISqoQuery<TRet> Select<TSource, TRet>(this ISqoQuery<TSource> self, Expression<Func<TSource, TRet>> selector)
		{
            return self.SqoSelect<TRet>(selector);
		}
		public static ISqoQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this ISqoQuery<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
		{
            return outer.SqoJoin<TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector, resultSelector);
            
		}
        public static int Count<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoCount();
        }

#if ASYNC
        public static Task<int> CountAsync<TSource>(this ISqoQuery<TSource> source)
        {
            return  source.SqoCountAsync();
        }
#endif

 

        
        public static int Count<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoCount(expression);
        }
#if ASYNC
        public static Task<int> CountAsync<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoCountAsync(expression);
        }
#endif

        public static TSource FirstOrDefault<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoFirstOrDefault();
        }
#if ASYNC
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoFirstOrDefaultAsync();
        }
#endif
 
        public static TSource FirstOrDefault<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoFirstOrDefault(expression);
        }
#if ASYNC
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoFirstOrDefaultAsync(expression);
        }
#endif
        public static TSource First<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoFirst();
        }
#if ASYNC
        public static Task<TSource> FirstAsync<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoFirstAsync();
        }
#endif
        public static TSource First<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoFirst(expression);
        }
#if ASYNC
        public static Task<TSource> FirstAsync<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoFirstAsync(expression);
        }
#endif
        public static bool Any<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoAny();
            
        }
#if ASYNC
        public static Task<bool> AnyAsync<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoAnyAsync();
        }
#endif
        public static bool Any<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoAny(expression);
        }
#if ASYNC
        public static Task<bool> AnyAsync<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoAnyAsync(expression);
        }
#endif
        public static TSource Last<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoLast();
        }
#if ASYNC
        public static Task<TSource> LastAsync<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoLastAsync();
        }

#endif
        public static TSource Last<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoLast(expression);
        }
#if ASYNC
        public static Task<TSource> LastAsync<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoLastAsync(expression);
        }
#endif
        public static TSource LastOrDefault<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoLastOrDefault();
        }
#if ASYNC
        public static Task<TSource> LastOrDefaultAsync<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoLastOrDefaultAsync();
        }
#endif
        public static TSource LastOrDefault<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoLastOrDefault(expression);
        }
#if ASYNC
        public static Task<TSource> LastOrDefaultAsync<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoLastOrDefaultAsync(expression);
        }
#endif
        public static TSource Single<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoSingle();

        }
#if ASYNC
        public static Task<TSource> SingleAsync<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoSingleAsync();

        }
#endif
        public static TSource Single<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoSingle(expression);
            
        }
#if ASYNC
        public static Task<TSource> SingleAsync<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {

            return source.SqoSingleAsync(expression);

        }
#endif
        public static TSource SingleOrDefault<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoSingleOrDefault();
        }
#if ASYNC
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this ISqoQuery<TSource> source)
        {
            return source.SqoSingleOrDefaultAsync();
        }
#endif
        public static TSource SingleOrDefault<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoSingleOrDefault(expression);
        }
#if ASYNC
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return source.SqoSingleOrDefaultAsync(expression);
        }
#endif
        public static ISqoQuery<TSource> Take<TSource>(this ISqoQuery<TSource> source, int count)
        {
            return source.SqoTake(count);
        }
#if ASYNC
        public static Task<ISqoQuery<TSource>> TakeAsync<TSource>(this ISqoQuery<TSource> source, int count)
        {
            return source.SqoTakeAsync(count);
        }
#endif
        public static ISqoQuery<TSource> Skip<TSource>(this ISqoQuery<TSource> source, int count)
        {
            return source.SqoSkip(count);
        }
#if ASYNC
        public static Task<ISqoQuery<TSource>> SkipAsync<TSource>(this ISqoQuery<TSource> source, int count)
        {
            return source.SqoSkipAsync(count);
        }
#endif
        public static ISqoQuery<TSource> Include<TSource>(this ISqoQuery<TSource> source, string path)
        {
            return source.SqoInclude(path);
        }
#if !UNITY3D

        public static ISqoOrderedQuery<TSource> OrderBy<TSource, TKey>(this ISqoQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return source.SqoOrderBy<TKey>(keySelector);

        }
        public static ISqoOrderedQuery<TSource> OrderByDescending<TSource, TKey>(this ISqoQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return source.SqoOrderByDescending<TKey>(keySelector);

        }
        public static ISqoOrderedQuery<TSource> ThenBy<TSource, TKey>(this ISqoOrderedQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return source.SqoThenBy<TKey>(keySelector);
          
        }
        public static ISqoOrderedQuery<TSource> ThenByDescending<TSource, TKey>(this ISqoOrderedQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return source.SqoThenByDescending<TKey>(keySelector);
        }
#endif   
       
    }
}
