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

namespace Sqo
{
    internal class QueryTranslator : ExpressionVisitor
    {

        ICriteria criteria;
        Dictionary<Expression, ICriteria> criteriaValues = new Dictionary<Expression, ICriteria>();
        Where currentWhere;
        StorageEngine engine;
        SqoTypeInfo ti;
        
        internal QueryTranslator(StorageEngine engine,SqoTypeInfo ti)
        {
            this.engine = engine;
            this.ti = ti;
        }
		
		internal QueryTranslator(bool justValidate)
		{
			justValidate=true;
		}



        internal ICriteria  Translate(Expression expression)
        {

            
            expression = Evaluator.PartialEval(expression);
            this.Visit(expression);

            return this.criteria;

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
            else if (typeof(IList).IsAssignableFrom(m.Method.DeclaringType) )
            {
                HandleIListMethods(m);
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
            else if (m.Method.DeclaringType == typeof(string) )
            {


                HandleStringMethods(m);
                return m;

            }
            else if ((m.Method.DeclaringType == typeof(SqoStringExtensions) && m.Method.Name == "Contains"))
            {
                HandleStringContainsMethod(m);
                return m;
            }
            throw new Exceptions.LINQUnoptimizeException(string.Format("The method '{0}' is not supported", m.Method.Name));

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
                case ExpressionType.Or:

                    HandleOr(b);

                    break;
                case ExpressionType.OrElse:

                    HandleOr(b);

                    break;

                case ExpressionType.Equal:
                    HandleWhere(b,OperationType.Equal);
                    break;
                case ExpressionType.NotEqual:
                    HandleWhere(b,OperationType.NotEqual);
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

            //this.Visit(b.Right);

         

            return b;

        }

        private  void HandleWhere(BinaryExpression b,OperationType opType)
        {
         
            
            Where w = new Where();
            w.StorageEngine = this.engine;
            w.ParentSqoTypeInfo = this.ti;
            criteriaValues[b] = w;
            currentWhere = w;
            w.OperationType = opType;

            if (criteria == null)
            {
                criteria = w;
			}
			this.Visit(b.Left);
			this.Visit(b.Right);

			#region not used
			//MethodCallExpression methodLeft = b.Left as MethodCallExpression;
			//MethodCallExpression methodRight = b.Right as MethodCallExpression;
			//if (methodLeft != null)
			//{
			//    try
			//    {
			//        object val = Expression.Lambda(b.Left).Compile().DynamicInvoke();
			//        w.Value = val;
			//    }
			//    catch
			//    {
			//        throw new Exception("Unoptimized method call");
			//    }
               
			//    this.Visit(b.Right);
			//}
			//else if (methodRight != null)
			//{
			//    this.Visit(b.Left);
			//    try
			//    {
			//        object val = Expression.Lambda(b.Right).Compile().DynamicInvoke();
			//        w.Value = val;
			//    }
			//    catch (Exception ex)
			//    {
			//        throw new Exception("Unoptimized method call");
			//    }
                
			//}
			//else
			//{
			//    this.Visit(b.Left);
			//    this.Visit(b.Right);
			//}
			#endregion

		}
        private void HandleIListMethods(MethodCallExpression m)
        {
            Where w = new Where();
            w.StorageEngine = this.engine;
            w.ParentSqoTypeInfo = this.ti;
            criteriaValues[m] = w;
            currentWhere = w;


            if (criteria == null)
            {
                criteria = w;
            }

            MemberExpression mExpression = m.Object as MemberExpression;
            if (mExpression == null)
            {
                throw new SiaqodbException("Must be a member that use IList method:" + m.Method.Name);
            }
            Visit(mExpression);
            ConstantExpression c = m.Arguments[0] as ConstantExpression;
            Visit(c);

            switch (m.Method.Name)
            {
                case "Contains":
                    {

                        w.OperationType = OperationType.Contains;
                        break;
                    }
                //case "Count":
                //    {

                //        w.OperationType = OperationType.ArrayLength;
                //        break;
                //    }
                //case "Length":
                //    {

                //        w.OperationType = OperationType.ArrayLength;
                //        break;
                //    }
                default:
                    throw new Exceptions.LINQUnoptimizeException("Unsupported string filtering query expression detected. ");
            }
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
                if (c2.Value != null && c2.Value.GetType() == typeof(string) && (mExpression.Member.Name == "StrTags" || mExpression.Member.Name == "IntTags"))
                {
                    currentWhere.Value2 = c2.Value;//KEY of dictionary
                }
                else
                {
                    throw new Exceptions.LINQUnoptimizeException("Unsupported string filtering query expression detected. ");
                }

            }
            else
            {
                Where w = new Where();
                w.StorageEngine = this.engine;
                w.ParentSqoTypeInfo = this.ti;
                criteriaValues[m] = w;
                currentWhere = w;


                if (criteria == null)
                {
                    criteria = w;
                }

                MemberExpression mExpression = m.Object as MemberExpression;
                if (mExpression == null)
                {
                    throw new SiaqodbException("Must be a member that use IDictionary method:" + m.Method.Name);
                }
                Visit(mExpression);
                ConstantExpression c = m.Arguments[0] as ConstantExpression;
                Visit(c);


                switch (m.Method.Name)
                {
                    case "ContainsKey":
                        {

                            w.OperationType = OperationType.ContainsKey;
                            break;
                        }
                    case "ContainsValue":
                        {

                            w.OperationType = OperationType.ContainsValue;
                            break;
                        }


                    default:
                        throw new Exceptions.LINQUnoptimizeException("Unsupported string filtering query expression detected. ");
                }
            }
        }
        private void HandleStringMethods(MethodCallExpression m)
        {
            Where w = new Where();
            w.StorageEngine = this.engine;
            w.ParentSqoTypeInfo = this.ti;
            criteriaValues[m] = w;
            currentWhere = w;
           

            if (criteria == null)
            {
                criteria = w;
            }

            MemberExpression mExpression = m.Object as MemberExpression;
            if (mExpression == null)
            {
                throw new SiaqodbException("Must be a member that use String method:" + m.Method.Name);
            }
            Visit(mExpression);
            ConstantExpression c = m.Arguments[0] as ConstantExpression;
            Visit(c);
            if (m.Arguments.Count == 2)
            {
                ConstantExpression c2 = m.Arguments[1] as ConstantExpression;
                if (c2.Value != null && c2.Value.GetType() == typeof(StringComparison))
                {
                    w.Value2 = (StringComparison)c2.Value;
                }
            }
            
            switch (m.Method.Name)
            {
                case "Contains":
                    {

                        w.OperationType = OperationType.Contains;
                        break;
                    }
                case "StartsWith":
                    {
                        w.OperationType = OperationType.StartWith;
                        break;
                    }
                case "EndsWith":
                    {
                        w.OperationType = OperationType.EndWith;
                        break;
                    }
                default:
                    throw new Exceptions.LINQUnoptimizeException("Unsupported string filtering query expression detected. ");
            }
        }
        private void HandleStringContainsMethod(MethodCallExpression m)
        {
            Where w = new Where();
            w.StorageEngine = this.engine;
            w.ParentSqoTypeInfo = this.ti;
            criteriaValues[m] = w;
            currentWhere = w;
            if (criteria == null)
            {
                criteria = w;
            }
            Visit(m.Arguments[0]);
            ConstantExpression c = m.Arguments[1] as ConstantExpression;
            Visit(c);

            ConstantExpression c2 = m.Arguments[2] as ConstantExpression;
            if (c2.Value != null && c2.Value.GetType() == typeof(StringComparison))
            {
                w.Value2 = (StringComparison)c2.Value;
            }
            w.OperationType = OperationType.Contains;


        }
        private void HandleAnd(BinaryExpression b)
        {
            Expression left = b.Left;
            Expression right = b.Right;

            #region handle alone boolean value
            MemberExpression leftMember=b.Left as MemberExpression;
            if (leftMember!=null)
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
            
            if (criteria == null)
            {
                criteria = new And();
                this.Visit(left);
                this.Visit(right);
                InvocationExpression iExpreLeft = left as InvocationExpression;
                if (iExpreLeft != null)
                    right = ((LambdaExpression)iExpreLeft.Expression).Body;

                InvocationExpression iExpreRight = right as InvocationExpression;
                if (iExpreRight != null)
                    right = ((LambdaExpression)iExpreRight.Expression).Body;

                ((And)criteria).Add(criteriaValues[left], criteriaValues[right]);
            }
            else
            {
                And newCriteria = new And();
                this.Visit(left);
                this.Visit(right);
                InvocationExpression iExpreLeft = left as InvocationExpression;
                if (iExpreLeft != null)
                    right = ((LambdaExpression)iExpreLeft.Expression).Body;

                InvocationExpression iExpreRight = right as InvocationExpression;
                if (iExpreRight != null)
                    right = ((LambdaExpression)iExpreRight.Expression).Body;


                newCriteria.Add(criteriaValues[left], criteriaValues[right]);
                criteriaValues[b] = newCriteria;
            }

            
        }
        private void HandleOr(BinaryExpression b)
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
            if (criteria == null)
            {
                criteria = new Or();
                this.Visit(left);
                this.Visit(right);
                ((Or)criteria).Add(criteriaValues[left], criteriaValues[right]);
            }
            else
            {
                Or newCriteria = new Or();
                this.Visit(left);
                this.Visit(right);
                newCriteria.Add(criteriaValues[left], criteriaValues[right]);
                criteriaValues[b] = newCriteria;
            }

           
        }
        protected override Expression VisitConstant(ConstantExpression c)
        {


            if (currentWhere == null)
            {
                throw new LINQUnoptimizeException("Unoptimized exception!");
            }
            currentWhere.Value = c.Value;
            //sb.Append(c.Value);



            return c;

        }



        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            //TODO when member is declared by interface, this would solve, but need to find out implementors of interface to get backing field of property
            //if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Convert || m.Expression.NodeType == ExpressionType.ConvertChecked))
            //{
            //    MemberExpression memExpr= Expression.MakeMemberAccess(((UnaryExpression)m.Expression).Operand ,m.Member) ;
            //   return this.VisitMemberAccess(memExpr);
               
            //}

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
					if (m.Member.Name == "OID")
					{
                        currentWhere.AttributeName.Add("OID");
						currentWhere.ParentType.Add( m.Member.DeclaringType);
					}
					else
					{
                        if (m.Member.DeclaringType == typeof(string))
                        {
                            throw new Exceptions.LINQUnoptimizeException(string.Format("The member '{0}' is not supported", m.Member.Name));

                        }
						System.Reflection.PropertyInfo pi = m.Member as System.Reflection.PropertyInfo;
						#if SILVERLIGHT || CF || UNITY3D || WinRT || MONODROID
                        string fieldName = SilverlightPropertyResolver.GetPrivateFieldName(pi, pi.DeclaringType);
                        if (fieldName != null)
                        {
                            currentWhere.AttributeName.Add( fieldName);
                            currentWhere.ParentType.Add( m.Member.DeclaringType);
                            

                        }
                        else
                        {
                            string fld = Sqo.Utilities.MetaHelper.GetBackingFieldByAttribute(m.Member);
                            if (fld != null)
                            {
                               
                                currentWhere.AttributeName.Add( fld);

                                currentWhere.ParentType.Add(m.Member.DeclaringType);
                            }
                            else
                            {
                                throw new SiaqodbException("A Property must have UseVariable Attribute set( property:" + m.Member.Name+" of type:"+m.Member.DeclaringType.ToString()+")");
                       
                            }
                        }

#else
                        try
                        {
                            System.Reflection.FieldInfo fi = BackingFieldResolver.GetBackingField(pi);
                            if (fi != null)
                            {
                                currentWhere.AttributeName.Add( fi.Name);

                                currentWhere.ParentType.Add(m.Member.DeclaringType);
                            }
                        }
                        catch
                        {
                            string fld = Sqo.Utilities.MetaHelper.GetBackingFieldByAttribute(m.Member);
                            if (fld != null)
                            {

                                currentWhere.AttributeName.Add( fld);

                                currentWhere.ParentType.Add(m.Member.DeclaringType);
                            }
                            else
                            {
                                throw new SiaqodbException("A Property must have UseVariable Attribute set( property:" + m.Member.Name+" of type:"+m.Member.DeclaringType.ToString()+")");
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
					currentWhere.AttributeName.Add( m.Member.Name);
					currentWhere.ParentType.Add(m.Member.DeclaringType);
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
           
            throw new  Exceptions.LINQUnoptimizeException(string.Format("The member '{0}' is not supported", m.Member.Name));

        }

    }
}
