using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo.PropertyResolver;

namespace Sqo
{
    class JoinTranslator : ExpressionVisitor
    {
        string joinFieldName;
        
        internal string Translate(Expression expression)
        {
            joinFieldName = null;
            expression = Evaluator.PartialEval(expression);
            this.Visit(expression);

            return this.joinFieldName;

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
						joinFieldName = m.Member.Name;
					}
					else
					{
                        System.Reflection.PropertyInfo pi = m.Member as System.Reflection.PropertyInfo;
#if SILVERLIGHT || CF || UNITY3D || WinRT
                        string fieldName = SilverlightPropertyResolver.GetPrivateFieldName(pi, pi.DeclaringType);
                        if (fieldName != null)
                        {
                            joinFieldName = fieldName;
                        }
                        else
                        {
                            string fld = Sqo.Utilities.MetaHelper.GetBackingFieldByAttribute(m.Member);
                            if (fld!=null)
                            {
                              
                                joinFieldName = fld;
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
                                joinFieldName = fi.Name;
                            }
                        }

                        catch (Exception ex)
                        {

                            string fld = Sqo.Utilities.MetaHelper.GetBackingFieldByAttribute(m.Member);
                            if (fld!=null)
                            {
                                
                                joinFieldName = fld;
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
					joinFieldName = m.Member.Name;
				}
				else throw new NotSupportedException("Unsupported Member Type!");
               
                return m;

            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));

        }

		
	}
}
