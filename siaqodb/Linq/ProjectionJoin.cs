using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;

using Sqo.Meta;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo.PropertyResolver;
using Sqo.Utilities;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo
{

    [System.Reflection.Obfuscation(Exclude = true)]
    public abstract class ProjectionRow
	{

        public abstract object GetValue(int index);

	}

    [System.Reflection.Obfuscation(Exclude = true)]
	public class ColumnProjection
	{
		internal List<SqoColumn> Columns;
		internal Expression Selector;
	}

    [System.Reflection.Obfuscation(Exclude = true)]
	public class ColumnProjector : ExpressionVisitor
	{
		List<SqoColumn> columns;
		int iColumn;
		ParameterExpression row;
		static MethodInfo miGetValue;
		internal ColumnProjector()
		{
			if (miGetValue == null)
			{
				miGetValue = typeof(ProjectionRow).GetMethod("GetValue");
			}
		}

		internal ColumnProjection ProjectColumns(Expression expression, ParameterExpression row)
		{
			this.columns = new List<SqoColumn>();
			this.row = row;
			Expression selector = this.Visit(expression);
			return new ColumnProjection { Columns = this.columns, Selector = selector };
		}

		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
			{
#if WinRT
                if (m.Member.GetMemberType() == MemberTypes.Property)
#else
                 if (m.Member.MemberType == System.Reflection.MemberTypes.Property)
#endif
				{
					if (m.Member.Name == "OID")
					{
						SqoColumn col = new SqoColumn();
						col.SourcePropName = m.Member.Name;
                        col.SourceType = m.Expression.Type;
						this.columns.Add(col);
						return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(iColumn++)), m.Type);

					}
					else
					{
						 System.Reflection.PropertyInfo pi = m.Member as System.Reflection.PropertyInfo;
						#if SILVERLIGHT || CF || UNITY3D || WinRT || MONODROID
                        string fieldName = SilverlightPropertyResolver.GetPrivateFieldName(pi, pi.DeclaringType);
                        if (fieldName != null)
                        {
                            SqoColumn col = new SqoColumn();
                            col.SourcePropName = fieldName;
                            col.SourceType = m.Expression.Type;
                            this.columns.Add(col);
                            return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(iColumn++)), m.Type);

                        }
                        else
                        {
                            string fld = MetaHelper.GetBackingFieldByAttribute(m.Member);
                            if (fld != null)
                            {
                                SqoColumn col = new SqoColumn();
                                col.SourcePropName = fld;
                                col.SourceType = m.Expression.Type;
                                this.columns.Add(col);
                                return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(iColumn++)), m.Type);

                            }
                            else
                            {
                                throw new SiaqodbException("A Property must have UseVariable Attribute set");
                            }
                        }

#else
                         try
                         {
                             System.Reflection.FieldInfo fi = BackingFieldResolver.GetBackingField(pi);
                             if (fi != null)
                             {
                                 SqoColumn col = new SqoColumn();
                                 col.SourcePropName = fi.Name;
                                 col.SourceType = m.Expression.Type;
                                 this.columns.Add(col);
                                 return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(iColumn++)), m.Type);

                             }
                             else
                             { 
                                throw  new SiaqodbException("A Property must have UseVariable Attribute set");
                             }
                         }
                         catch
                         {
                             string fld = Sqo.Utilities.MetaHelper.GetBackingFieldByAttribute(m.Member);
                             if (fld != null)
                             {
                                 
                                 SqoColumn col = new SqoColumn();
                                 col.SourcePropName = fld;
                                 col.SourceType = m.Expression.Type;
                                 this.columns.Add(col);
                                 return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(iColumn++)), m.Type);

                             }
                             else
                             {
                                 throw new SiaqodbException("A Property must have UseVariable Attribute set");
                             }
                         }
#endif
					}
				}
#if WinRT
                else if (m.Member.GetMemberType() == MemberTypes.Field)
#else
                else if (m.Member.MemberType == System.Reflection.MemberTypes.Field)
#endif
				{
					SqoColumn col = new SqoColumn();
					col.SourcePropName = m.Member.Name;
                    col.SourceType = m.Expression.Type;
					this.columns.Add(col);
					return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(iColumn++)), m.Type);

				}
				else throw new NotSupportedException("Not supported Member Type!");

			}
			else
			{
				return base.VisitMemberAccess(m);
			}
		}
		protected override Expression VisitParameter(ParameterExpression p)
		{
			SqoColumn col = new SqoColumn();
			col.SourcePropName = p.Name;
			col.SourceType = p.Type;
			col.IsFullObject = true;
			this.columns.Add(col);
			return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(iColumn++)), p.Type);

			
		}
	}
	internal class TranslateResult
	{

		internal List<SqoColumn> Columns;
		internal LambdaExpression Projector;
	}

	internal class QueryTranslatorProjection : ExpressionVisitor
	{
		ParameterExpression row;
		ColumnProjection projection;

		internal QueryTranslatorProjection()
		{
		}

		private static Expression StripQuotes(Expression e)
		{
			while (e.NodeType == ExpressionType.Quote)
			{
				e = ((UnaryExpression)e).Operand;
			}
			return e;

		}
		internal TranslateResult Translate(Expression m)
		{
			m = Evaluator.PartialEval(m);
           
			this.row = Expression.Parameter(typeof(ProjectionRow), "row");
			LambdaExpression lambda = m as LambdaExpression;
			if (lambda == null)
			{
				throw new Exceptions.LINQUnoptimizeException("Expression is type:" + m.NodeType.ToString() + " and not LambdaExpression");
			}
			ColumnProjection projection = new ColumnProjector().ProjectColumns(lambda.Body, this.row);
			
			this.projection = projection;

			return new TranslateResult
			{

				Columns = projection.Columns,
				Projector = this.projection != null ? Expression.Lambda(this.projection.Selector, this.row) : null

			};
		}

	
	}

    [System.Reflection.Obfuscation(Exclude = true)]
	class ProjectionReader<T,TOuter,TInner> : ISqoQuery<T>
	{

		Enumerator enumerator;

		Expression outerExpression;
		Expression innerExpression;
		ISqoQuery<TOuter> SqoQueryOuter;
		ISqoQuery<TInner> SqoQueryInner;


		public ProjectionReader(List<SqoColumn> columns, Func<ProjectionRow, T> projector,ISqoQuery<TOuter> SqoQueryOuter,ISqoQuery<TInner> SqoQueryInner,Expression outer,Expression inner)
		{
			this.enumerator = new Enumerator(columns, projector);
			this.outerExpression = outer;
			this.innerExpression = inner;
			this.SqoQueryInner = SqoQueryInner;
			this.SqoQueryOuter = SqoQueryOuter;
		}
#if ASYNC
        public async Task<IList<T>> ToListAsync()
        {
            Enumerator e = this.enumerator;
            SqoQuery<TOuter> SqoQueryOuterImp = SqoQueryOuter as SqoQuery<TOuter>;
            SqoQuery<TInner> SqoQueryInnerImp = SqoQueryInner as SqoQuery<TInner>;
            if (SqoQueryOuterImp != null)
            {
                List<KeyValuePair<int, int>> oids = await SqoQueryOuterImp.Siaqodb.LoadOidsForJoinAsync<T, TOuter, TInner>(SqoQueryOuterImp, SqoQueryInnerImp, outerExpression, innerExpression);

                e.oids = oids;
                e.siaqodb = SqoQueryOuterImp.Siaqodb;
                e.outerType = typeof(TOuter);
                e.innerType = typeof(TInner);
                List<T> list = new List<T>();
                while (e.MoveNext())
                {
                    list.Add(e.Current);
                }
                return list;
            }
            else
            {
                throw new LINQUnoptimizeException("cannot optimize");
            }
            //this.enumerator = null;

        }
#endif
		public IEnumerator<T> GetEnumerator()
		{
			Enumerator e = this.enumerator;
			SqoQuery<TOuter> SqoQueryOuterImp = SqoQueryOuter as SqoQuery<TOuter>;
			SqoQuery<TInner> SqoQueryInnerImp = SqoQueryInner as SqoQuery<TInner>;
			if (SqoQueryOuterImp != null)
			{
				List<KeyValuePair<int, int>> oids = SqoQueryOuterImp.Siaqodb.LoadOidsForJoin<T, TOuter, TInner>(SqoQueryOuterImp, SqoQueryInnerImp, outerExpression, innerExpression);

				e.oids = oids;
				e.siaqodb = SqoQueryOuterImp.Siaqodb;
				e.outerType = typeof(TOuter);
				e.innerType = typeof(TInner);
				
			}
			else
			{
                throw new LINQUnoptimizeException("cannot optimize");
			}
			//this.enumerator = null;
			return e;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator, IDisposable
		{
			List<SqoColumn> columns;
			T current;
			Func<ProjectionRow, T> projector;
			internal List<KeyValuePair<int, int>> oids;
			int currentIndex = 0;
			internal Siaqodb siaqodb;
			internal Type outerType;
			internal Type innerType;
			internal Enumerator(List<SqoColumn> columns, Func<ProjectionRow, T> projector)
			{
				this.columns = columns;
				this.projector = projector;
			}
			public override object GetValue(int index)
			{

				SqoColumn col = columns[index];
				if (col.SourceType == innerType)
				{
					if (col.IsFullObject)
					{
						return siaqodb.LoadObjectByOID(col.SourceType, oids[currentIndex].Value);
					}
					else
					{
						return siaqodb.LoadValue(oids[currentIndex].Value, col.SourcePropName, col.SourceType);
					}
				}
				else
				{
					if (col.IsFullObject)
					{
						return siaqodb.LoadObjectByOID(col.SourceType, oids[currentIndex].Key);
					}
					else
					{
						return siaqodb.LoadValue(oids[currentIndex].Key, col.SourcePropName, col.SourceType);
					}
				}
				
				
			}

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

			public void Reset()
			{
                this.currentIndex = 0;
			}
			public void Dispose()
			{

			}


        }


     
    }


}
