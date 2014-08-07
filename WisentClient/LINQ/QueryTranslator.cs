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

        Criteria currentWhere;
        List<Criteria> criterias;
        Dictionary<Expression, Criteria> criteriaValues = new Dictionary<Expression, Criteria>();
      
        internal QueryTranslator()
        {
           
        }
		
		internal QueryTranslator(bool justValidate)
		{
			justValidate=true;
		}



        internal List<Criteria> Translate(Expression expression)
        {
            
            expression = Evaluator.PartialEval(expression);
            this.Visit(expression);

            return this.criterias;

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
           
            else if (m.Method.DeclaringType==typeof(Sqo.CryptonorObject) && m.Method.Name=="Tags")
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
                    HandleWhere(b,Criteria.Equal);
                    break;
                case ExpressionType.LessThan:
                    HandleWhere(b, Criteria.LessThan);
                    break;
                case ExpressionType.LessThanOrEqual:
                    HandleWhere(b, Criteria.LessThanOrEqual);
                    break;
                case ExpressionType.GreaterThan:
                    HandleWhere(b,Criteria.GreaterThan);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    HandleWhere(b, Criteria.GreaterThanOrEqual);
                    break;
                                
                 
                default:

                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            return b;

        }

        private  void HandleWhere(BinaryExpression b,string opType)
        {
            Criteria w = new Criteria();
            w.OperationType = opType;
            currentWhere = w;
            criteriaValues[b] = w;
          
            if (criterias == null)
            {
                criterias = new List<Criteria>();
               
            }
            criterias.Add(w);
            this.Visit(b.Left);
            this.Visit(b.Right);

		}

        private void HandleDictionaryMethods(MethodCallExpression m)
        {

            ConstantExpression c2 = m.Arguments[0] as ConstantExpression;
            if (c2.Value != null && c2.Value.GetType() == typeof(string) &&
                (m.Method.ReturnType == typeof(int) || m.Method.ReturnType == typeof(long) || m.Method.ReturnType == typeof(DateTime) || m.Method.ReturnType == typeof(bool)
            || m.Method.ReturnType == typeof(string) || m.Method.ReturnType == typeof(double) || m.Method.ReturnType == typeof(float))
                )
            {
                currentWhere.TagName = c2.Value.ToString();//KEY of dictionary
                if (m.Method.ReturnType == typeof(int) || m.Method.ReturnType == typeof(long))
                {
                    currentWhere.TagType = "tags_int";
                }
                else if (m.Method.ReturnType == typeof(DateTime))
                {
                    currentWhere.TagType = "tags_datetime";
                }
                else if (m.Method.ReturnType == typeof(double) || m.Method.ReturnType == typeof(float))
                {
                    currentWhere.TagType = "tags_double";
                }
                else if (m.Method.ReturnType == typeof(string))
                {
                    currentWhere.TagType = "tags_string";
                }
                else if (m.Method.ReturnType == typeof(bool))
                {
                    currentWhere.TagType = "tags_bool";
                }
            }
            else
            {
                throw new LINQUnoptimizeException("Unsupported string filtering query expression detected. ");
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
            this.Visit(left);
            this.Visit(right);


        }
       
        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (currentWhere == null)
            {
                throw new LINQUnoptimizeException("Unoptimized exception!");
            }

            currentWhere.TagValue = c.Value;

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
