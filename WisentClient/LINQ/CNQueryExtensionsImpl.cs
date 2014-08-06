using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Sqo.Exceptions;
using Sqo.Utilities;
#if ASYNC
using System.Threading.Tasks;
using Sqo;
#endif
namespace CryptonorClient
{

    internal static class CNQueryExtensionsImpl
    {

        public static Sqo.ISqoQuery<TSource> Where<TSource>(Sqo.ISqoQuery<TSource> self, Expression<Func<TSource, bool>> expression)
        {
            try
            {
                QueryTranslator qt = new QueryTranslator(true);
                qt.Validate(expression);
                CNQuery<Sqo.CryptonorObject> CNQuery = self as CNQuery<Sqo.CryptonorObject>;
                if (CNQuery == null)
                    throw new LINQUnoptimizeException();
                if (CNQuery.Expression != null)
                {
                    CNQuery.Expression = Merge2ExpressionsByAnd<TSource>(CNQuery.Expression, expression);
                }
                else
                {
                    CNQuery.Expression = expression;
                }

                return (Sqo.ISqoQuery<TSource>)CNQuery;
            }
            catch (LINQUnoptimizeException)
            {
                throw;              
#if (WP7 || UNITY3D) && !MANGO && !XIOS
                Func<TSource, bool> fn = (Func<TSource, bool>)ExpressionCompiler.ExpressionCompiler.Compile(expression);
#else

                Func<TSource, bool> fn = expression.Compile();
#endif

                //return new SelectQueryWhere<TSource>(fn, self);
            }

        }

        private static Expression Merge2ExpressionsByAnd<TSource>(Expression expr1, Expression<Func<TSource, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, ((LambdaExpression)expr1).Parameters.Cast<Expression>());
            return Expression.Lambda<Func<TSource, bool>>
                  (Expression.AndAlso(((LambdaExpression)expr1).Body, invokedExpr), ((LambdaExpression)expr1).Parameters);
        }
		public static Sqo.ISqoQuery<TRet> Select<TSource, TRet>(Sqo.ISqoQuery<TSource> self, Expression<Func<TSource, TRet>> selector)
		{
#if ASYNC
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
		public static Sqo.ISqoQuery<TResult> Join<TOuter, TInner, TKey, TResult>(Sqo.ISqoQuery<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
		{
            throw new NotImplementedException();
           
            
		}
        public static int Count<TSource>(Sqo.ISqoQuery<TSource> source)
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

#if ASYNC
        public static async Task<int> CountAsync<TSource>(Sqo.ISqoQuery<TSource> source)
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

 

        
        public static int Count<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
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
#if ASYNC
        public static async Task<int> CountAsync<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
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

        public static TSource FirstOrDefault<TSource>(Sqo.ISqoQuery<TSource> source)
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
#if ASYNC
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(Sqo.ISqoQuery<TSource> source)
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

 
        public static TSource FirstOrDefault<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
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
#if ASYNC
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
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
        public static TSource First<TSource>(Sqo.ISqoQuery<TSource> source)
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
#if ASYNC
        public static async Task<TSource> FirstAsync<TSource>(Sqo.ISqoQuery<TSource> source)
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
        public static TSource First<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);

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
#if ASYNC
        public static async Task<TSource> FirstAsync<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);

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
        public static bool Any<TSource>(Sqo.ISqoQuery<TSource> source)
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
#if ASYNC
        public static async Task<bool> AnyAsync<TSource>(Sqo.ISqoQuery<TSource> source)
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
        public static bool Any<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
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
#if ASYNC
        public static async Task<bool> AnyAsync<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
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
        public static TSource Last<TSource>(Sqo.ISqoQuery<TSource> source)
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
#if ASYNC
        public static async Task<TSource> LastAsync<TSource>(Sqo.ISqoQuery<TSource> source)
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
        public static TSource Last<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
              SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
              if (sqoQ != null)
              {
                  return sqoQ.GetLast(true);
              }
              return ((IEnumerable<TSource>)query).Last<TSource>();
        }
#if ASYNC
        public static async Task<TSource> LastAsync<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return await sqoQ.GetLastAsync(true);
            }
            return ((IEnumerable<TSource>)query).Last<TSource>();
        }
#endif
        public static TSource LastOrDefault<TSource>(Sqo.ISqoQuery<TSource> source)
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
#if ASYNC
        public static async Task<TSource> LastOrDefaultAsync<TSource>(Sqo.ISqoQuery<TSource> source)
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
        public static TSource LastOrDefault<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return sqoQ.GetLast(false);
            }
            return ((IEnumerable<TSource>)query).LastOrDefault<TSource>();
        }
#if ASYNC
        public static async Task<TSource> LastOrDefaultAsync<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
            SqoQuery<TSource> sqoQ = query as SqoQuery<TSource>;
            if (sqoQ != null)
            {
                return await sqoQ.GetLastAsync(false);
            }
            return ((IEnumerable<TSource>)query).LastOrDefault<TSource>();
        }
#endif
        public static TSource Single<TSource>(Sqo.ISqoQuery<TSource> source)
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
#if ASYNC
        public static async Task<TSource> SingleAsync<TSource>(Sqo.ISqoQuery<TSource> source)
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
        public static TSource Single<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);
           
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
#if ASYNC
        public static async Task<TSource> SingleAsync<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);

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
        public static TSource SingleOrDefault<TSource>(Sqo.ISqoQuery<TSource> source)
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
#if ASYNC
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(Sqo.ISqoQuery<TSource> source)
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
        public static TSource SingleOrDefault<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);

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
#if ASYNC
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(Sqo.ISqoQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Sqo.ISqoQuery<TSource> query = Where<TSource>(source, expression);

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
        public static Sqo.ISqoQuery<TSource> Take<TSource>(Sqo.ISqoQuery<TSource> source, int count)
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
           
            return new SelectQuery<TSource>(((IEnumerable<TSource>)source).Take<TSource>(count));
        }
#if ASYNC
        public static async Task<Sqo.ISqoQuery<TSource>> TakeAsync<TSource>(Sqo.ISqoQuery<TSource> source, int count)
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
        public static Sqo.ISqoQuery<TSource> Skip<TSource>(Sqo.ISqoQuery<TSource> source, int count)
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
           
            return new SelectQuery<TSource>(((IEnumerable<TSource>)source).Skip<TSource>(count));
        }
#if ASYNC
        public static async Task<Sqo.ISqoQuery<TSource>> SkipAsync<TSource>(Sqo.ISqoQuery<TSource> source, int count)
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
           
            return null;
        }
#endif
        

    }
}
