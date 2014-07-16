using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Sqo.Queries;
using Sqo.Meta;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo.PropertyResolver;
using System.Collections;
using System.Reflection;

namespace CryptonorClient
{
    internal class QueryTranslator : ExpressionVisitor
    {

        Where currentWhere;
          
        internal QueryTranslator()
        {
           
        }
		
		internal QueryTranslator(bool justValidate)
		{
			justValidate=true;
		}



        internal Where Translate(Expression expression)
        {

            
            expression = Evaluator.PartialEval(expression);
            this.Visit(expression);

            return this.currentWhere;

        }
		internal void Validate(Expression expression)
		{
			expression = Evaluator.PartialEval(expression);
			this.Visit(expression);
		}
        


        private static Expression StripQuotes(Expression e)
        {

            while (e.NodeType == ExpressionType.Quote)
            {

                e = ((UnaryExpression)e).Operand;

            }

            return e;

        }



        protected override Expression VisitMethodCall(MethodCallExpression m)
        {

			if (m.Method.DeclaringType == typeof(IEnumerable) && m.Method.Name == "Where")
            {

                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                this.Visit(lambda.Body);

                return m;

            }
           
            else if (typeof(IDictionary).IsAssignableFrom(m.Method.DeclaringType))
            {
                HandleDictionaryMethods(m);
                return m;
            }
            else if (m.Method.DeclaringType == typeof(IEnumerable) && m.Method.Name == "Select")
            {

            }
           
            throw new Exception(string.Format("The method '{0}' is not supported", m.Method.Name));

        }

     



        protected override Expression VisitUnary(UnaryExpression u)
        {

            switch (u.NodeType)
            {

                case ExpressionType.Not:

                    throw new LINQUnoptimizeException("Unary operaor not yet supported");    
                case ExpressionType.Convert:

                    this.Visit(u.Operand);

                    break;
                

                default:

                    throw new LINQUnoptimizeException("Unary operator not yet supported");

            }

            return u;

        }



        protected override Expression VisitBinary(BinaryExpression b)
        {
            
            switch (b.NodeType)
            {

                case ExpressionType.And:

                    HandleAnd(b);
                    break;
                case ExpressionType.AndAlso:

                    HandleAnd(b);

                    break;

                case ExpressionType.Equal:
                    HandleWhere(b,OperationType.Equal);
                    break;
                case ExpressionType.LessThan:
                    HandleWhere(b,OperationType.LessThan);
                    break;
                case ExpressionType.LessThanOrEqual:
                    HandleWhere(b,OperationType.LessThanOrEqual);
                    break;
                case ExpressionType.GreaterThan:
                    HandleWhere(b,OperationType.GreaterThan);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    HandleWhere(b,OperationType.GreaterThanOrEqual);
                    break;
                                
                 
                default:

                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            return b;

        }

        private  void HandleWhere(BinaryExpression b,OperationType opType)
        {
            Where w = new Where();
            w.OperationType = opType;
            if (this.currentWhere == null)
                this.currentWhere = w;
			this.Visit(b.Left);
			this.Visit(b.Right);

		}

        private void HandleDictionaryMethods(MethodCallExpression m)
        {
            if (m.Method.Name == "get_Item")
            {
                MemberExpression mExpression = m.Object as MemberExpression;
                if (mExpression == null)
                {
                    throw new SiaqodbException("Must be a member that use IDictionary method:" + m.Method.Name);
                }
                Visit(mExpression);
                ConstantExpression c2 = m.Arguments[0] as ConstantExpression;
                if (c2.Value != null && c2.Value.GetType() == typeof(string) &&
                    (mExpression.Member.Name == "Tags_Int" || mExpression.Member.Name == "Tags_DateTime"
                    || mExpression.Member.Name == "Tags_String" || mExpression.Member.Name == "Tags_Double"
                    || mExpression.Member.Name == "Tags_Bool")
                    )
                {
                    currentWhere.TagName = c2.Value.ToString();//KEY of dictionary
                }
                else
                {
                    throw new LINQUnoptimizeException("Unsupported string filtering query expression detected. ");
                }

            }

        
        }

        private void HandleAnd(BinaryExpression b)
        {
            Expression left = b.Left;
            Expression right = b.Right;

            #region handle alone boolean value
            MemberExpression leftMember = b.Left as MemberExpression;
            if (leftMember != null)
            {
                if (leftMember.Expression != null && leftMember.Expression.NodeType == ExpressionType.Parameter)
                {
                    if (leftMember.Type == typeof(bool)) //ex: WHERE .. && Active
                    {
                        BinaryExpression exp = BinaryExpression.MakeBinary(ExpressionType.Equal, leftMember, Expression.Constant(true));
                        left = exp;
                    }
                }
            }
            MemberExpression rightMember = b.Right as MemberExpression;
            if (rightMember != null)
            {
                if (rightMember.Expression != null && rightMember.Expression.NodeType == ExpressionType.Parameter)
                {
                    if (rightMember.Type == typeof(bool)) //ex: WHERE .. && Active
                    {
                        BinaryExpression exp = BinaryExpression.MakeBinary(ExpressionType.Equal, rightMember, Expression.Constant(true));
                        right = exp;
                    }
                }
            }
            #endregion

            if (currentWhere == null)
            {
                currentWhere = new Where();
            }
            this.Visit(left);
            this.Visit(right);
            InvocationExpression iExpreLeft = left as InvocationExpression;
            if (iExpreLeft != null)
                right = ((LambdaExpression)iExpreLeft.Expression).Body;

            InvocationExpression iExpreRight = right as InvocationExpression;
            if (iExpreRight != null)
                right = ((LambdaExpression)iExpreRight.Expression).Body;




        }
      
        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (currentWhere == null)
            {
                throw new LINQUnoptimizeException("Unoptimized exception!");
            }
            if (currentWhere.TagValue != null)
            {
                currentWhere.TagValue2 = c.Value;
            }
            else
            {
                currentWhere.TagValue = c.Value;
            }
            return c;

        }



        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            

            if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.MemberAccess))
            {

                if (currentWhere == null)
                {
                    if (m.Type == typeof(bool)) //ex: WHERE Active
                    {
                        BinaryExpression exp= BinaryExpression.MakeBinary(ExpressionType.Equal,m,Expression.Constant(true));
                        return this.VisitBinary(exp);
                    }
                }
#if WinRT
                if (m.Member.GetMemberType() == MemberTypes.Property)
#else
                if (m.Member.MemberType == System.Reflection.MemberTypes.Property)
#endif
				{
                    if (m.Member.Name == "Tags_Int" || m.Member.Name == "Tags_DateTime"
                   || m.Member.Name == "Tags_String" || m.Member.Name == "Tags_Double"
                   || m.Member.Name == "Tags_Bool")
                    {
                        currentWhere.TagType = (TagType)Enum.Parse(typeof(TagType), m.Member.Name);
                    }
					
				}

				else throw new NotSupportedException("Unsupported Member Type!");

                if (m.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    return base.VisitMemberAccess(m);
                }
                else
                {
                    return m;
                }

            }
           
            throw new  LINQUnoptimizeException(string.Format("The member '{0}' is not supported", m.Member.Name));

        }

    }
}
