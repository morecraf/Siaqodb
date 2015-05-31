using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Sqo.Exceptions;
using Sqo.Utilities;
#if ASYNC_LMDB
using System.Threading.Tasks;
#endif

namespace Sqo
{
    
    internal static class SqoQueryExtensionsImpl
    {

        public static ISqoQuery<TSource> Where<TSource>(ISqoQuery<TSource> self, Expression<Func<TSource, bool>> expression)
        {

            try
            {

                QueryTranslator qt = new QueryTranslator(true);
                qt.Validate(expression);
                SqoQuery<TSource> SqoQuery = self as SqoQuery<TSource>;
                if (SqoQuery == null)
                    throw new Exceptions.LINQUnoptimizeException();
                if (SqoQuery.Expression != null)
                {
                    SqoQuery.Expression = Merge2ExpressionsByAnd<TSource>(SqoQuery.Expression, expression);
                }
                else
                {
                    SqoQuery.Expression = expression;
                }
               
                return SqoQuery;
            }
            catch (Exceptions.LINQUnoptimizeException)
            {
                SiaqodbConfigurator.LogMessage("Expression:" + expression.ToString() + " cannot be parsed, query runs un-optimized!",VerboseLevel.Warn);
				#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Func<TSource, bool> fn = (Func<TSource, bool>)ExpressionCompiler.ExpressionCompiler.Compile(expression);
#else

                Func<TSource, bool> fn = expression.Compile();
#endif

                return new SelectQueryWhere<TSource>(fn, self);
            }

        }

        private static Expression Merge2ExpressionsByAnd<TSource>(Expression expr1, Expression<Func<TSource, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, ((LambdaExpression)expr1).Parameters.Cast<Expression>());
            return Expression.Lambda<Func<TSource, bool>>
                  (Expression.AndAlso(((LambdaExpression)expr1).Body, invokedExpr), ((LambdaExpression)expr1).Parameters);
        }
		public static ISqoQuery<TRet> Select<TSource, TRet>(ISqoQuery<TSource> self, Expression<Func<TSource, TRet>> selector)
		{
#if ASYNC_LMDB
            Func<TSource, TRet> fn = selector.Compile();


            return new SelectQuery<TSource, TRet>(fn, self);
#else
            if (typeof(TSource) == typeof(TRet))
            {
				#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Func<TSource, TRet> fn = (Func<TSource, TRet>)ExpressionCompiler.ExpressionCompiler.Compile(selector);
#else

                Func<TSource, TRet> fn = selector.Compile();
#endif

                return new SelectQuery<TSource, TRet>(fn, self);
			}
			try
			{
				SqoQuery<TSource> to = self as SqoQuery<TSource>;
				//SelectQuery<TSource> toSel = self as SelectQuery<TSource>;
				if (to == null )
				{
					throw new Exceptions.LINQUnoptimizeException("MultiJoint not yet supported");
				}
				QueryTranslatorProjection qp = new QueryTranslatorProjection();
				TranslateResult result = qp.Translate(selector);
				#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Delegate projector = ExpressionCompiler.ExpressionCompiler.Compile(result.Projector);
#else

                Delegate projector = result.Projector.Compile();
#endif

                Type elementType = typeof(TRet);


				Type t = typeof(ProjectionSelectReader<,>).MakeGenericType(elementType, typeof(TSource));
				ConstructorInfo ctor = t.GetConstructor(new Type[] { typeof(List<SqoColumn>), typeof(Func<ProjectionRow, TRet>), typeof(SqoQuery<TSource>) });
				ProjectionSelectReader<TRet, TSource> r = (ProjectionSelectReader<TRet, TSource>)ctor.Invoke(new object[] { result.Columns, projector, (SqoQuery<TSource>)self });


				return r;
			}
			catch (Exceptions.LINQUnoptimizeException ex3)
            {
                SiaqodbConfigurator.LogMessage("Expression:" + selector.ToString() + " cannot be parsed, query runs un-optimized!", VerboseLevel.Warn);
				#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Func<TSource, TRet> fn = (Func<TSource, TRet>)ExpressionCompiler.ExpressionCompiler.Compile(selector);
#else

                Func<TSource, TRet> fn = selector.Compile();
#endif
                return  new SelectQuery<TSource, TRet>(fn, self);
			}
			#if SILVERLIGHT
            catch (MethodAccessException ex)
            {
                throw new SiaqodbException("Siaqodb on Silverlight not support anonymous types, please use a strong Type ");
            }
            #endif
#endif
		}
		public static ISqoQuery<TResult> Join<TOuter, TInner, TKey, TResult>(ISqoQuery<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
		{
		    ISqoQuery<TOuter> SqoQueryOuter = (ISqoQuery<TOuter>)outer;
		    ISqoQuery<TInner> SqoQueryInner = (ISqoQuery<TInner>)inner;

            try
            {
                SqoQuery<TOuter> to = SqoQueryOuter as SqoQuery<TOuter>;
                SelectQuery<TResult, TOuter> toSel = SqoQueryOuter as SelectQuery<TResult,TOuter>;
                if (to == null && toSel == null)
                {
                    throw new Exceptions.LINQUnoptimizeException("MultiJoin not yet supported");
                }
                SqoQuery<TInner> tinn = SqoQueryInner as SqoQuery<TInner>;
                SelectQuery<TResult, TInner> tinnSel = SqoQueryInner as SelectQuery<TResult,TInner>;
                if (tinn == null && tinnSel == null)
                {
                    throw new Exceptions.LINQUnoptimizeException("MultiJoin not yet supported");
                }

                QueryTranslatorProjection qp = new QueryTranslatorProjection();

                TranslateResult result = qp.Translate(resultSelector);
				#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Delegate projector = ExpressionCompiler.ExpressionCompiler.Compile(result.Projector);
#else

                Delegate projector = result.Projector.Compile();
#endif
               
                Type elementType = typeof(TResult);

                Type t = typeof(ProjectionReader<,,>).MakeGenericType(elementType, typeof(TOuter), typeof(TInner));
                ConstructorInfo ctor = t.GetConstructor(new Type[] { typeof(List<SqoColumn>), typeof(Func<ProjectionRow, TResult>), typeof(ISqoQuery<TOuter>), typeof(ISqoQuery<TInner>), typeof(Expression), typeof(Expression) });
                ProjectionReader<TResult, TOuter, TInner> r = (ProjectionReader<TResult, TOuter, TInner>)ctor.Invoke(new object[] { result.Columns, projector, SqoQueryOuter, SqoQueryInner, outerKeySelector, innerKeySelector });


                return r;
            }
            catch (Exceptions.LINQUnoptimizeException ex3)
            {
                SiaqodbConfigurator.LogMessage("Expression:" + resultSelector.ToString() + " cannot be parsed, query runs un-optimized!", VerboseLevel.Warn);
				#if (WP7 || UNITY3D) && !MANGO && !XIOS 
                Func<TOuter, TKey> outerKeySelectorFN = (Func<TOuter, TKey>)ExpressionCompiler.ExpressionCompiler.Compile(outerKeySelector);
                Func<TInner, TKey> innerKeySelectorFN = (Func<TInner, TKey>)ExpressionCompiler.ExpressionCompiler.Compile(innerKeySelector);
                Func<TOuter, TInner, TResult> resultSelectorFN = (Func<TOuter, TInner, TResult>)ExpressionCompiler.ExpressionCompiler.Compile(resultSelector);
#else

                Func<TOuter, TKey> outerKeySelectorFN  = outerKeySelector.Compile();
             Func<TInner, TKey> innerKeySelectorFN  = innerKeySelector.Compile();
             Func<TOuter, TInner, TResult> resultSelectorFN=resultSelector.Compile();
#endif

             return new SelectQueryJoin<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelectorFN, innerKeySelectorFN, resultSelectorFN);
            }
            #if SILVERLIGHT
            catch (MethodAccessException ex)
            {
                throw new SiaqodbException("Siaqodb on Silverlight not support anonymous types, please use a strong Type ");
            }
            #endif
            catch (Exception ex)
            {
                throw ex;

            }
           
            
		}
        public static int Count<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
           SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return sqoQ.CountOids();
            }
            ICollection<TSource> collection = source as ICollection<TSource>;
            if (collection != null)
            {
                return collection.Count;
            }

            int num = 0;
            foreach (TSource t in source)
            {
                num++;
            }
            return num;
        }

#if ASYNC_LMDB
        public static async Task<int> CountAsync<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return await sqoQ.CountOidsAsync();
            }
            ICollection<TSource> collection = source as ICollection<TSource>;
            if (collection != null)
            {
                return collection.Count;
            }

            int num = 0;
            foreach (TSource t in source)
            {
                num++;
            }
            return num;
        }
#endif

 

        
        public static int Count<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return sqoQ.CountOids();
            }
            ICollection<TSource> collection = query as ICollection<TSource>;
            if (collection != null)
            {
                return collection.Count;
            }

            int num = 0;
            foreach (TSource t in query)
            {
                num++;
            }
            return num;
        }
#if ASYNC_LMDB
        public static async Task<int> CountAsync<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return await sqoQ.CountOidsAsync();
            }
            ICollection<TSource> collection = query as ICollection<TSource>;
            if (collection != null)
            {
                return collection.Count;
            }

            int num = 0;
            foreach (TSource t in query)
            {
                num++;
            }
            return num;
        }
#endif

        public static TSource FirstOrDefault<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
           
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> sqoIncludeQ = source as IncludeSqoQuery<TSource>;
           
            if (sqoQ != null)
            {
                IEnumerator<TSource> lazyEnum = sqoQ.GetLazyEnumerator();
                if (lazyEnum.MoveNext())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    return default(TSource);
                }
            }
            else if (sqoIncludeQ != null)
            {
                IEnumerator<TSource> lazyEnum = sqoIncludeQ.GetEnumerator();
                if (lazyEnum.MoveNext())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    return default(TSource);
                }
            }
            return ((IEnumerable<TSource>)source).FirstOrDefault<TSource>();
        }
#if ASYNC_LMDB
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> sqoIncludeQ = source as IncludeSqoQuery<TSource>;
            if (sqoQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await sqoQ.GetLazyEnumeratorAsync();
                if (await lazyEnum.MoveNextAsync())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    return default(TSource);
                }
            }
            else if (sqoIncludeQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await sqoIncludeQ.GetEnumeratorAsync();
                if (await lazyEnum.MoveNextAsync())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    return default(TSource);
                }
            }
            return ((IEnumerable<TSource>)source).FirstOrDefault<TSource>();
        }
#endif

 
        public static TSource FirstOrDefault<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                IEnumerator<TSource> lazyEnum = sqoQ.GetLazyEnumerator();
                if (lazyEnum.MoveNext())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    return default(TSource);
                }
            }

            using (IEnumerator<TSource> enumerator = query.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            return default(TSource);
    

        }
#if ASYNC_LMDB
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await sqoQ.GetLazyEnumeratorAsync();
                if (await lazyEnum.MoveNextAsync())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    return default(TSource);
                }
            }

            using (IEnumerator<TSource> enumerator = query.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            return default(TSource);


        }
#endif
        public static TSource First<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> sqoIncludeQ = source as IncludeSqoQuery<TSource>;
            if (sqoQ != null)
            {
                IEnumerator<TSource> lazyEnum = sqoQ.GetLazyEnumerator();
                if (lazyEnum.MoveNext())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    throw new InvalidOperationException("The source sequence is empty.");
                }
            }
            else if (sqoIncludeQ != null)
            {
                IEnumerator<TSource> lazyEnum = sqoIncludeQ.GetEnumerator();
                if (lazyEnum.MoveNext())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    throw new InvalidOperationException("The source sequence is empty.");
                }
            }
            return ((IEnumerable<TSource>)source).First<TSource>();
        }
#if ASYNC_LMDB
        public static async Task<TSource> FirstAsync<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> sqoIncludeQ = source as IncludeSqoQuery<TSource>;
            if (sqoQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await sqoQ.GetLazyEnumeratorAsync();
                if (await lazyEnum.MoveNextAsync())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    throw new InvalidOperationException("The source sequence is empty.");
                }
            }
            else if (sqoIncludeQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await sqoIncludeQ.GetEnumeratorAsync();
                if (await lazyEnum.MoveNextAsync())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    throw new InvalidOperationException("The source sequence is empty.");
                }
            }
            return ((IEnumerable<TSource>)source).First<TSource>();
        }
#endif
        public static TSource First<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);

            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                IEnumerator<TSource> lazyEnum = sqoQ.GetLazyEnumerator();
                if (lazyEnum.MoveNext())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    throw new InvalidOperationException("The source sequence is empty.");
                }
            }
            using (IEnumerator<TSource> enumerator = query.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            throw new InvalidOperationException("The source sequence is empty.");
        }
#if ASYNC_LMDB
        public static async Task<TSource> FirstAsync<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);

            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await sqoQ.GetLazyEnumeratorAsync();
                if (await lazyEnum.MoveNextAsync())
                {
                    return lazyEnum.Current;
                }
                else
                {
                    throw new InvalidOperationException("The source sequence is empty.");
                }
            }
            using (IEnumerator<TSource> enumerator = query.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            throw new InvalidOperationException("The source sequence is empty.");
        }
#endif
        public static bool Any<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                IEnumerator<TSource> lazyEnum = sqoQ.GetLazyEnumerator();
                if (lazyEnum.MoveNext())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
            
        }
#if ASYNC_LMDB
        public static async Task<bool> AnyAsync<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await sqoQ.GetLazyEnumeratorAsync();
                if (await lazyEnum.MoveNextAsync())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;

        }
#endif
        public static bool Any<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                IEnumerator<TSource> lazyEnum = sqoQ.GetLazyEnumerator();
                if (lazyEnum.MoveNext())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            using (IEnumerator<TSource> enumerator = query.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }
#if ASYNC_LMDB
        public static async Task<bool> AnyAsync<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await sqoQ.GetLazyEnumeratorAsync();
                if (await lazyEnum.MoveNextAsync())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            using (IEnumerator<TSource> enumerator = query.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }
#endif
        public static TSource Last<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
          
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return sqoQ.GetLast(true);
            }
            return ((IEnumerable<TSource>)source).Last<TSource>();
        }
#if ASYNC_LMDB
        public static async Task<TSource> LastAsync<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return await sqoQ.GetLastAsync(true);
            }
            return ((IEnumerable<TSource>)source).Last<TSource>();
        }

#endif
        public static TSource Last<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
              SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
              if (sqoQ != null)
              {
                  return sqoQ.GetLast(true);
              }
              return ((IEnumerable<TSource>)query).Last<TSource>();
        }
#if ASYNC_LMDB
        public static async Task<TSource> LastAsync<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return await sqoQ.GetLastAsync(true);
            }
            return ((IEnumerable<TSource>)query).Last<TSource>();
        }
#endif
        public static TSource LastOrDefault<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return sqoQ.GetLast(false);
            }
            return ((IEnumerable<TSource>)source).LastOrDefault<TSource>();
        }
#if ASYNC_LMDB
        public static async Task<TSource> LastOrDefaultAsync<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return await sqoQ.GetLastAsync(false);
            }
            return ((IEnumerable<TSource>)source).LastOrDefault<TSource>();
        }
#endif
        public static TSource LastOrDefault<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return sqoQ.GetLast(false);
            }
            return ((IEnumerable<TSource>)query).LastOrDefault<TSource>();
        }
#if ASYNC_LMDB
        public static async Task<TSource> LastOrDefaultAsync<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return await sqoQ.GetLastAsync(false);
            }
            return ((IEnumerable<TSource>)query).LastOrDefault<TSource>();
        }
#endif
        public static TSource Single<TSource>(ISqoQuery<TSource> source)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
           
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> includeSqoQ = source as IncludeSqoQuery<TSource>;
            if (sqoQ != null)
            {
                List<int> oids = sqoQ.GetOids();
                if (oids.Count == 1)
                {
                    return sqoQ.Siaqodb.LoadObjectByOID<TSource>(oids[0]);
                }
                if (oids.Count == 0)
                {
                    throw new InvalidOperationException("No match");
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            else if (includeSqoQ != null)
            {
                IEnumerator<TSource> lazyEnum = includeSqoQ.GetEnumerator();
                int i = 0;
                TSource obj = default(TSource);
                while(lazyEnum.MoveNext())
                {
                    obj =lazyEnum.Current;
                    i++;
                    if (i > 1)
                        break;
                }
                if (i == 1)
                {
                    return obj;
                }
                else if (i == 0)
                {
                    throw new InvalidOperationException("No match");
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            return ((IEnumerable<TSource>)source).Single<TSource>();

        }
#if ASYNC_LMDB
        public static async Task<TSource> SingleAsync<TSource>(ISqoQuery<TSource> source)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> includeSqoQ = source as IncludeSqoQuery<TSource>;
           
            if (sqoQ != null)
            {
                List<int> oids = await sqoQ.GetOidsAsync();
                if (oids.Count == 1)
                {
                    return await sqoQ.Siaqodb.LoadObjectByOIDAsync<TSource>(oids[0]);
                }
                if (oids.Count == 0)
                {
                    throw new InvalidOperationException("No match");
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            else if (includeSqoQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await includeSqoQ.GetEnumeratorAsync();
                int i = 0;
                TSource obj = default(TSource);
                while (await lazyEnum.MoveNextAsync())
                {
                    obj = lazyEnum.Current;
                    i++;
                    if (i > 1)
                        break;
                }
                if (i == 1)
                {
                    return obj;
                }
                else if (i == 0)
                {
                    throw new InvalidOperationException("No match");
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            return ((IEnumerable<TSource>)source).Single<TSource>();

        }
#endif
        public static TSource Single<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);
           
             SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
             if (sqoQ != null)
             {
                 List<int> oids = sqoQ.GetOids();
                 if (oids.Count == 1)
                 {
                     return sqoQ.Siaqodb.LoadObjectByOID<TSource>(oids[0]);
                 }
                 if (oids.Count == 0)
                 {
                     throw new InvalidOperationException("No match");
                 }
                 else
                 {
                     throw new InvalidOperationException("Many matches");
                 }
             }
             return ((IEnumerable<TSource>)query).Single<TSource>();
            
        }
#if ASYNC_LMDB
        public static async Task<TSource> SingleAsync<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);

            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                List<int> oids = await sqoQ.GetOidsAsync();
                if (oids.Count == 1)
                {
                    return await sqoQ.Siaqodb.LoadObjectByOIDAsync<TSource>(oids[0]);
                }
                if (oids.Count == 0)
                {
                    throw new InvalidOperationException("No match");
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            return ((IEnumerable<TSource>)query).Single<TSource>();

        }
#endif
        public static TSource SingleOrDefault<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
           
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> includeSqoQ = source as IncludeSqoQuery<TSource>;
           
            if (sqoQ != null)
            {
                List<int> oids = sqoQ.GetOids();
                if (oids.Count == 1)
                {
                    return sqoQ.Siaqodb.LoadObjectByOID<TSource>(oids[0]);
                }
                if (oids.Count == 0)
                {
                    return default(TSource);
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            else if (includeSqoQ != null)
            {
                IEnumerator<TSource> lazyEnum = includeSqoQ.GetEnumerator();
                int i = 0;
                TSource obj = default(TSource);
                while (lazyEnum.MoveNext())
                {
                    obj = lazyEnum.Current;
                    i++;
                    if (i > 1)
                        break;
                }
                if (i == 1)
                {
                    return obj;
                }
                else if (i == 0)
                {
                    return default(TSource);
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            return ((IEnumerable<TSource>)source).SingleOrDefault<TSource>();
        }
#if ASYNC_LMDB
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(ISqoQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> includeSqoQ = source as IncludeSqoQuery<TSource>;
           
            if (sqoQ != null)
            {
                List<int> oids = await sqoQ.GetOidsAsync();
                if (oids.Count == 1)
                {
                    return await sqoQ.Siaqodb.LoadObjectByOIDAsync<TSource>(oids[0]);
                }
                if (oids.Count == 0)
                {
                    return default(TSource);
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            else if (includeSqoQ != null)
            {
                ISqoAsyncEnumerator<TSource> lazyEnum = await includeSqoQ.GetEnumeratorAsync();
                int i = 0;
                TSource obj = default(TSource);
                while (await lazyEnum.MoveNextAsync())
                {
                    obj = lazyEnum.Current;
                    i++;
                    if (i > 1)
                        break;
                }
                if (i == 1)
                {
                    return obj;
                }
                else if (i == 0)
                {
                    return default(TSource);
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            return ((IEnumerable<TSource>)source).SingleOrDefault<TSource>();
        }
#endif
        public static TSource SingleOrDefault<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);

            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                List<int> oids = sqoQ.GetOids();
                if (oids.Count == 1)
                {
                    return sqoQ.Siaqodb.LoadObjectByOID<TSource>(oids[0]);
                }
                if (oids.Count == 0)
                {
                    return default(TSource);
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            return ((IEnumerable<TSource>)query).SingleOrDefault<TSource>();
        }
#if ASYNC_LMDB
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ISqoQuery<TSource> query = Where<TSource>(source, expression);

            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                List<int> oids = await sqoQ.GetOidsAsync();
                if (oids.Count == 1)
                {
                    return await sqoQ.Siaqodb.LoadObjectByOIDAsync<TSource>(oids[0]);
                }
                if (oids.Count == 0)
                {
                    return default(TSource);
                }
                else
                {
                    throw new InvalidOperationException("Many matches");
                }
            }
            return ((IEnumerable<TSource>)query).SingleOrDefault<TSource>();
        }
#endif
        public static ISqoQuery<TSource> Take<TSource>(ISqoQuery<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
           
            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                if (count <= 0)
                {
                    return source;
                }
               
                List<int> oids = sqoQ.GetOids();
                if (oids.Count <= count)
                {
                    return source;
                }
                int[] oidsArr = new int[count];
                oids.CopyTo(0, oidsArr, 0,  count);
                return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>(oidsArr));
            }
            LazySqoQuery<TSource> lazyQ = source as LazySqoQuery<TSource>;
            if (lazyQ != null)
            {
                if (count <= 0)
                {
                    return source;
                }

                List<int> oids = lazyQ.GetOids();
                if (oids.Count <= count)
                {
                    return source;
                }
                int[] oidsArr = new int[count];
                oids.CopyTo(0, oidsArr, 0, count);
                return new LazySqoQuery<TSource>(lazyQ.Siaqodb, new List<int>(oidsArr));
            }
            SqoOrderedQuery<TSource> orderedQuery = source as SqoOrderedQuery<TSource>;
            if (orderedQuery != null)
            {
                if (count <= 0)
                {
                    return source;
                }

                List<int> oids = orderedQuery.SortAndGetOids();
                if (oids.Count <= count)
                {
                    return source;
                }
                int[] oidsArr = new int[count];
                oids.CopyTo(0, oidsArr, 0, count);
                return new LazySqoQuery<TSource>(orderedQuery.siaqodb, new List<int>(oidsArr));
            }
            return new SelectQuery<TSource>(((IEnumerable<TSource>)source).Take<TSource>(count));
        }
#if ASYNC_LMDB
        public static async Task<ISqoQuery<TSource>> TakeAsync<TSource>(ISqoQuery<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                if (count <= 0)
                {
                    return source;
                }

                List<int> oids = await sqoQ.GetOidsAsync();
                if (oids.Count <= count)
                {
                    return source;
                }
                int[] oidsArr = new int[count];
                oids.CopyTo(0, oidsArr, 0, count);
                return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>(oidsArr));
            }
            LazySqoQuery<TSource> lazyQ = source as LazySqoQuery<TSource>;
            if (lazyQ != null)
            {
                if (count <= 0)
                {
                    return source;
                }

                List<int> oids = lazyQ.GetOids();
                if (oids.Count <= count)
                {
                    return source;
                }
                int[] oidsArr = new int[count];
                oids.CopyTo(0, oidsArr, 0, count);
                return new LazySqoQuery<TSource>(lazyQ.Siaqodb, new List<int>(oidsArr));
            }
            return null;
        }
#endif
        public static ISqoQuery<TSource> Skip<TSource>(ISqoQuery<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {

                if (count <= 0)
                {
                    return source;
                }
                List<int> oids = sqoQ.GetOids();
                if (count >= oids.Count)
                {
                    return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>());
                }
                else
                {
                    int[] oidsArr = new int[oids.Count - count];
                    oids.CopyTo(count, oidsArr, 0, oids.Count - count);
                    return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>(oidsArr));
                }
            }
            LazySqoQuery<TSource> lazySqo = source as LazySqoQuery<TSource>;
            if (lazySqo != null)
            {

                if (count <= 0)
                {
                    return source;
                }
                List<int> oids = lazySqo.GetOids();
                if (count >= oids.Count)
                {
                    return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>());
                }
                else
                {
                    int[] oidsArr = new int[oids.Count - count];
                    oids.CopyTo(count, oidsArr, 0, oids.Count - count);
                    return new LazySqoQuery<TSource>(lazySqo.Siaqodb, new List<int>(oidsArr));
                }
            }
            SqoOrderedQuery<TSource> orderedQuery = source as SqoOrderedQuery<TSource>;
            if (orderedQuery != null)
            {
                if (count <= 0)
                {
                    return source;
                }
                List<int> oids = orderedQuery.SortAndGetOids();
                if (count >= oids.Count)
                {
                    return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>());
                }
                else
                {
                    int[] oidsArr = new int[oids.Count - count];
                    oids.CopyTo(count, oidsArr, 0, oids.Count - count);
                    return new LazySqoQuery<TSource>(orderedQuery.siaqodb, new List<int>(oidsArr));
                }
            }
            return new SelectQuery<TSource>(((IEnumerable<TSource>)source).Skip<TSource>(count));
        }
#if ASYNC_LMDB
        public static async Task<ISqoQuery<TSource>> SkipAsync<TSource>(ISqoQuery<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            if (sqoQ != null)
            {

                if (count <= 0)
                {
                    return source;
                }
                List<int> oids = await sqoQ.GetOidsAsync();
                if (count >= oids.Count)
                {
                    return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>());
                }
                else
                {
                    int[] oidsArr = new int[oids.Count - count];
                    oids.CopyTo(count, oidsArr, 0, oids.Count - count);
                    return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>(oidsArr));
                }
            }
            LazySqoQuery<TSource> lazySqo = source as LazySqoQuery<TSource>;
            if (lazySqo != null)
            {

                if (count <= 0)
                {
                    return source;
                }
                List<int> oids = lazySqo.GetOids();
                if (count >= oids.Count)
                {
                    return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>());
                }
                else
                {
                    int[] oidsArr = new int[oids.Count - count];
                    oids.CopyTo(count, oidsArr, 0, oids.Count - count);
                    return new LazySqoQuery<TSource>(lazySqo.Siaqodb, new List<int>(oidsArr));
                }
            }
            SqoOrderedQuery<TSource> orderedQuery = source as SqoOrderedQuery<TSource>;
            if (orderedQuery != null)
            {
                if (count <= 0)
                {
                    return source;
                }
                List<int> oids = orderedQuery.SortAndGetOids();
                if (count >= oids.Count)
                {
                    return new LazySqoQuery<TSource>(sqoQ.Siaqodb, new List<int>());
                }
                else
                {
                    int[] oidsArr = new int[oids.Count - count];
                    oids.CopyTo(count, oidsArr, 0, oids.Count - count);
                    return new LazySqoQuery<TSource>(orderedQuery.siaqodb, new List<int>(oidsArr));
                }
            }
            return null;
        }
#endif
        public static ISqoQuery<TSource> Include<TSource>(ISqoQuery<TSource> source, string path)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SqoQuery<TSource> sqoQ = source as SqoQuery<TSource>;
            IncludeSqoQuery<TSource> isqoQ = source as IncludeSqoQuery<TSource>;
            if (sqoQ != null)
            {
               
                return new IncludeSqoQuery<TSource>(sqoQ, path);
            }
            else if (isqoQ != null)
            {

                isqoQ.includes.Add(path);
                return isqoQ;

            }
            else
            {
                throw new Sqo.Exceptions.SiaqodbException("Include is only allowed on Where or other Include!");    
            }
            
        }
	#if !UNITY3D || XIOS

        public static ISqoOrderedQuery<TSource> OrderBy<TSource, TKey>(ISqoQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            ISqoQuery<TKey> select = Select<TSource, TKey>(source, keySelector);
            ProjectionSelectReader<TKey, TSource> r = select as ProjectionSelectReader<TKey, TSource>;
            SqoQuery<TSource> sqoQuery = source as SqoQuery<TSource>;
            if (r != null && sqoQuery != null)
            {
                EnumeratorSelect<TKey> selectEnum = r.GetEnumerator() as EnumeratorSelect<TKey>;
                List<int> selectOids = selectEnum.oids;
                List<SqoSortableItem> orderedList = new List<SqoSortableItem>(selectOids.Count);
                int i = 0;
                foreach (TKey enumItem in r)
                {
                    SqoSortableItem sortableItem = new SqoSortableItem(selectOids[i], enumItem);
                    orderedList.Add(sortableItem);
                    i++;
                }
                SqoComparer<SqoSortableItem> comparer = new SqoComparer<SqoSortableItem>(false);
                SqoOrderedQuery<TSource> orderedQuery = new SqoOrderedQuery<TSource>(sqoQuery.Siaqodb, orderedList, comparer);


                return orderedQuery;
            }
            SiaqodbConfigurator.LogMessage("Expression:" + keySelector.ToString() + " cannot be parsed, query runs un-optimized!", VerboseLevel.Warn);
	#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Func<TSource, TKey> fn = (Func<TSource, TKey>)ExpressionCompiler.ExpressionCompiler.Compile(keySelector);
#else

            Func<TSource, TKey> fn = keySelector.Compile();
#endif
            return new SqoObjOrderedQuery<TSource>(((IEnumerable<TSource>)source).OrderBy<TSource, TKey>(fn));

        }
        public static ISqoOrderedQuery<TSource> OrderByDescending<TSource, TKey>(ISqoQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            ISqoQuery<TKey> select = Select<TSource, TKey>(source, keySelector);
            ProjectionSelectReader<TKey, TSource> r = select as ProjectionSelectReader<TKey, TSource>;
            SqoQuery<TSource> sqoQuery = source as SqoQuery<TSource>;
            if (r != null && sqoQuery != null)
            {
                EnumeratorSelect<TKey> selectEnum = r.GetEnumerator() as EnumeratorSelect<TKey>;
                List<int> selectOids = selectEnum.oids;
                List<SqoSortableItem> orderedList = new List<SqoSortableItem>(selectOids.Count);
                int i = 0;
                foreach (TKey enumItem in r)
                {
                    SqoSortableItem sortableItem = new SqoSortableItem(selectOids[i], enumItem);
                    orderedList.Add(sortableItem);
                    i++;
                }
                SqoComparer<SqoSortableItem> comparer = new SqoComparer<SqoSortableItem>(true);
                SqoOrderedQuery<TSource> orderedQuery = new SqoOrderedQuery<TSource>(sqoQuery.Siaqodb, orderedList, comparer);


                return orderedQuery;
            }
            SiaqodbConfigurator.LogMessage("Expression:" + keySelector.ToString() + " cannot be parsed, query runs un-optimized!", VerboseLevel.Warn);
	#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Func<TSource, TKey> fn = (Func<TSource,TKey >)ExpressionCompiler.ExpressionCompiler.Compile(keySelector);
#else

            Func<TSource, TKey> fn = keySelector.Compile();
#endif
            return new SqoObjOrderedQuery<TSource>(((IEnumerable<TSource>)source).OrderByDescending<TSource, TKey>(fn));

        }
        public static ISqoOrderedQuery<TSource> ThenBy<TSource, TKey>( ISqoOrderedQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            SqoOrderedQuery<TSource> orderedQuery = source as SqoOrderedQuery<TSource>;
            if (orderedQuery != null)
            {
                QueryTranslatorProjection qp = new QueryTranslatorProjection();
                TranslateResult result = qp.Translate(keySelector);
                if (result.Columns.Count == 1)
                {
                    orderedQuery.comparer.AddOrder(false);
                    foreach (SqoSortableItem item in orderedQuery.SortableItems)
                    { 
                        item.Add(orderedQuery.siaqodb.LoadValue(item.oid,result.Columns[0].SourcePropName,typeof(TSource)));
                    }
                    return orderedQuery;
                }

            }
            SiaqodbConfigurator.LogMessage("Expression:" + keySelector.ToString() + " cannot be parsed, query runs un-optimized!", VerboseLevel.Warn);
	#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Func<TSource, TKey> fn = (Func<TSource, TKey>)ExpressionCompiler.ExpressionCompiler.Compile(keySelector);
#else

            Func<TSource, TKey> fn = keySelector.Compile();
#endif

            return new SqoObjOrderedQuery<TSource>(((IOrderedEnumerable<TSource>)source).ThenBy<TSource, TKey>(fn));
          
        }
        public static ISqoOrderedQuery<TSource> ThenByDescending<TSource, TKey>( ISqoOrderedQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            SqoOrderedQuery<TSource> orderedQuery = source as SqoOrderedQuery<TSource>;
            if (orderedQuery != null)
            {
                QueryTranslatorProjection qp = new QueryTranslatorProjection();
                TranslateResult result = qp.Translate(keySelector);
                if (result.Columns.Count == 1)
                {
                    orderedQuery.comparer.AddOrder(true);
                    foreach (SqoSortableItem item in orderedQuery.SortableItems)
                    {
                        item.Add(orderedQuery.siaqodb.LoadValue(item.oid, result.Columns[0].SourcePropName, typeof(TSource)));
                    }
                    return orderedQuery;
                }

            }
            SiaqodbConfigurator.LogMessage("Expression:" + keySelector.ToString() + " cannot be parsed, query runs un-optimized!", VerboseLevel.Warn);
	#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Func<TSource, TKey> fn = (Func<TSource, TKey>)ExpressionCompiler.ExpressionCompiler.Compile(keySelector);
#else

            Func<TSource, TKey> fn = keySelector.Compile();
#endif
            return new SqoObjOrderedQuery<TSource>(((IOrderedEnumerable<TSource>)source).ThenByDescending<TSource, TKey>(fn));

        }
#endif   
    }
}
