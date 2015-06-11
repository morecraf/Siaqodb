using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Dotissi
{

    [System.Reflection.Obfuscation(Exclude = true)]

    internal static class Evaluator
    {

        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>

        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }


        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
       /// </summary>

        /// <param name="expression">The root of the expression tree.</param>

        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>

        public static Expression PartialEval(Expression expression)
        {

            return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);

        }



        private static bool CanBeEvaluatedLocally(Expression expression)
        {

            return expression.NodeType != ExpressionType.Parameter;

        }



        /// <summary>

        /// Evaluates & replaces sub-trees when first candidate is reached (top-down)

        /// </summary>

        class SubtreeEvaluator : ExpressionVisitor
        {

            List<Expression> candidates;



            internal SubtreeEvaluator(List<Expression> candidates)
            {
				
                this.candidates = candidates;

            }



            internal Expression Eval(Expression exp)
            {

                return this.Visit(exp);

            }



            protected override Expression Visit(Expression exp)
            {

                if (exp == null)
                {

                    return null;

                }

                if (this.candidates.Contains(exp))
                {

                    return this.Evaluate(exp);

                }

                return base.Visit(exp);

            }



            private Expression Evaluate(Expression e)
            {

                if (e.NodeType == ExpressionType.Constant)
                {

                    return e;

                }
#if UNITY3D
                MemberExpression m = e as MemberExpression;
                
                if (m != null)
                {
                    Expression exp = m.Expression;

                    if (exp == null || exp is ConstantExpression) 
                    {
                        object obj = exp == null ? null : ((ConstantExpression)exp).Value;
                        object value = null; Type type = null;
                        if (m.Member is FieldInfo)
                        {
                            FieldInfo fi = (FieldInfo)m.Member;
                            value = fi.GetValue(@obj);
                            type = fi.FieldType;
                        }
                        else if (m.Member is PropertyInfo)
                        {
                            PropertyInfo pi = (PropertyInfo)m.Member;
                            if (pi.GetIndexParameters().Length != 0)
                                throw new ArgumentException("cannot eliminate closure references to indexed properties");
                            value = pi.GetGetMethod().Invoke(obj, null);
                            type = pi.PropertyType;
                        }
                        return Expression.Constant(value, type);
                    }
                }
               
#endif
                LambdaExpression lambda = Expression.Lambda(e);

				#if (WP7 || UNITY3D) && !MANGO  && !XIOS
                Delegate fn = ExpressionCompiler.ExpressionCompiler.Compile(lambda);
            #else

                Delegate fn = lambda.Compile();
            #endif

#if CF

                return Expression.Constant(fn.Method.Invoke(fn.Target, new object[0]),e.Type);
#else
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
                
#endif

			}

        }



        /// <summary>

        /// Performs bottom-up analysis to determine which nodes can possibly

        /// be part of an evaluated sub-tree.

        /// </summary>

        class Nominator : ExpressionVisitor
        {

            Func<Expression, bool> fnCanBeEvaluated;

            List<Expression> candidates;

            bool cannotBeEvaluated;



            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {

                this.fnCanBeEvaluated = fnCanBeEvaluated;

            }



            internal List<Expression> Nominate(Expression expression)
            {

                this.candidates = new List<Expression>();

                this.Visit(expression);

                return this.candidates;

            }



            protected override Expression Visit(Expression expression)
            {

                if (expression != null)
                {

                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;

                    this.cannotBeEvaluated = false;

                    base.Visit(expression);

                    if (!this.cannotBeEvaluated)
                    {

                        if (this.fnCanBeEvaluated(expression))
                        {

                            this.candidates.Add(expression);

                        }

                        else
                        {

                            this.cannotBeEvaluated = true;

                        }

                    }

                    this.cannotBeEvaluated |= saveCannotBeEvaluated;

                }

                return expression;

            }

        }

    }
}
