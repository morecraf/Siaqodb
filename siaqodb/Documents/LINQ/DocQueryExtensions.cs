using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace Sqo.Documents
{
    public static class DocQueryExtensions
    {
        private static readonly MethodInfo getMethod;
        private static readonly Dictionary<MethodInfo, MethodInfo> functionMappings;
        private static readonly MethodInfo stringContains;
        private static readonly MethodInfo stringStartsWith;
        private static readonly MethodInfo stringEndsWith;
        static DocQueryExtensions()
        {
            getMethod = GetMethod<Document>(obj => obj.GetTag<int>(null)).GetGenericMethodDefinition();
            stringContains = GetMethod<string>(str => str.Contains(null));
            stringStartsWith = GetMethod<string>(str => str.StartsWith(null));
            stringEndsWith = GetMethod<string>(str => str.EndsWith(null));
            functionMappings = new Dictionary<MethodInfo, MethodInfo> {
                {
                  stringContains,
                  GetMethod<Query>(q => q.WhereContains(null, null))
                },
                {
                  stringStartsWith,
                  GetMethod<Query>(q => q.WhereStartsWith(null, null))
                },
                {
                  stringEndsWith,
                  GetMethod<Query>(q => q.WhereEndsWith(null,null))
                },
            };
        }
        public static Document FirstOrDefault(this IDocQuery<Document> source) 
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source.Bucket.FindFirst(source.InnerQuery);
        }
        public static Document First(this IDocQuery<Document> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            var doc= source.Bucket.FindFirst(source.InnerQuery);
            if (doc != null)
                return doc;
            else
            {
                throw new InvalidOperationException("The source sequence is empty.");
            }
        }
        public static int Count(this IDocQuery<Document> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source.Bucket.Count(source.InnerQuery);
        }
        public static IDocQuery<TSource> Where<TSource>(this IDocQuery<TSource> source, Expression<Func<TSource, bool>> predicate)
        {

            var binaryExpression = predicate.Body as BinaryExpression;
            if (binaryExpression != null)
            {
                if (binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    return source
                        .Where(Expression.Lambda<Func<TSource, bool>>(
                            binaryExpression.Left, predicate.Parameters))
                        .Where(Expression.Lambda<Func<TSource, bool>>(
                            binaryExpression.Right, predicate.Parameters));
                }
                if (binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    var left = source.Where(Expression.Lambda<Func<TSource, bool>>(
                            binaryExpression.Left, predicate.Parameters));
                    Query q = null;
                    Type t = SiaqodbConfigurator.GetQueryTypeToBuild(source.Bucket.BucketName);
                    if (t == typeof(Query))
                    {
                        q = new Query();
                    }
                    else//subclass like IQryptQuery
                    {
                        q = Activator.CreateInstance(t) as Query;
                    }

                    IDocQuery<TSource> rightNew = new DocQuery<TSource>(source.Bucket, q);
                    var right = rightNew.Where(Expression.Lambda<Func<TSource, bool>>(
                            binaryExpression.Right, predicate.Parameters));


                    left.InnerQuery.Or(right.InnerQuery);
                    return left;
                }
            }
            var normalized = new WhereNormalizer().Visit(predicate.Body);

            var methodCallExpr = normalized as MethodCallExpression;
            if (methodCallExpr != null)
            {
                return source.WhereMethodCall(predicate, methodCallExpr);
            }

            var binaryExpr = normalized as BinaryExpression;
            if (binaryExpr != null)
            {
                return source.WhereBinaryExpression(predicate, binaryExpr);
            }

            var unaryExpr = normalized as UnaryExpression;
            if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Not)
            {
                var node = unaryExpr.Operand as MethodCallExpression;
                if (IsDocumentGetTag(node) && (node.Type == typeof(bool) || node.Type == typeof(bool?)))
                {
                    // This is a raw boolean field access like 'where !doc.GetTag<bool>("foo")'
                    source.InnerQuery.WhereNotEqual(GetValue(node.Arguments[0]) as string, true);
                    return source;
                }
            }

            throw new InvalidOperationException(
              "Encountered an unsupported expression for DocQuery.");
        }
        public static IDocQuery<TSource> OrderBy<TSource, TSelector>(this IDocQuery<TSource> source, Expression<Func<TSource, TSelector>> keySelector)
        {
            source.InnerQuery.OrderBy(GetOrderByPath(keySelector));
            return source;
        }
        public static IDocQuery<TSource> OrderByDescending<TSource, TSelector>(this IDocQuery<TSource> source, Expression<Func<TSource, TSelector>> keySelector)
        {
            source.InnerQuery.OrderByDesc(GetOrderByPath(keySelector));
            return source;
        }
        public static IDocQuery<TSource> ThenBy<TSource, TSelector>(this IDocQuery<TSource> source, Expression<Func<TSource, TSelector>> keySelector)
        {
             source.InnerQuery.ThenBy(GetOrderByPath(keySelector));
            return source;
        }
        public static IDocQuery<TSource> ThenByDescending<TSource, TSelector>(this IDocQuery<TSource> source, Expression<Func<TSource, TSelector>> keySelector)
        {
            source.InnerQuery.ThenByDesc(GetOrderByPath(keySelector));
            return source;
        }
        public static IDocQuery<TSource> Skip<TSource>(this IDocQuery<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            source.InnerQuery.skip = count;
            return source;
        }
        public static IDocQuery<TSource> Take<TSource>(this IDocQuery<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            source.InnerQuery.limit = count;
            return source;
        }

        private static string GetOrderByPath<TSource, TSelector>(Expression<Func<TSource, TSelector>> keySelector)
        {
            string result = null;
            var normalized = new ObjectNormalizer().Visit(keySelector.Body);
            var callExpr = normalized as MethodCallExpression;
            if (IsDocumentGetTag(callExpr) && callExpr.Object == keySelector.Parameters[0])
            {
                // We're operating on the parameter
                result = GetValue(callExpr.Arguments[0]) as string;
            }
            if (result == null)
            {
                throw new InvalidOperationException(
                  "OrderBy expression must be a tag of Document");
            }
            return result;
        }

        private static IDocQuery<T> WhereBinaryExpression<T>(
       this IDocQuery<T> source, Expression<Func<T, bool>> expression, BinaryExpression node)
        {
            var leftTransformed = new ObjectNormalizer().Visit(node.Left) as MethodCallExpression;

            if (!(IsDocumentGetTag(leftTransformed) &&
                leftTransformed.Object == expression.Parameters[0]))
            {
                throw new InvalidOperationException(
                  "Where expressions must have one side be a Tag of a Document.");
            }

            var fieldPath = GetValue(leftTransformed.Arguments[0]) as string;
            var filterValue = GetValue(node.Right);

            //TODO check if tagType is supported
            /*if (filterValue != null && !tag.IsValidType(filterValue))
            {
                throw new InvalidOperationException(
                  "Where clauses must use types compatible with Document.");
            }*/

            switch (node.NodeType)
            {
                case ExpressionType.GreaterThan:
                    source.InnerQuery.WhereGreaterThan(fieldPath, filterValue);
                    return source;
                case ExpressionType.GreaterThanOrEqual:
                    source.InnerQuery.WhereGreaterThanOrEqual(fieldPath, filterValue);
                    return source;
                case ExpressionType.LessThan:
                    source.InnerQuery.WhereLessThan(fieldPath, filterValue);
                    return source;
                case ExpressionType.LessThanOrEqual:
                    source.InnerQuery.WhereLessThanOrEqual(fieldPath, filterValue);
                    return source;
                case ExpressionType.Equal:
                    source.InnerQuery.WhereEqual(fieldPath, filterValue);
                    return source;
                case ExpressionType.NotEqual:
                    source.InnerQuery.WhereNotEqual(fieldPath, filterValue);
                    return source;
                default:
                    throw new InvalidOperationException(
                      "Where expressions do not support this operator.");
            }
        }
       
        private static IDocQuery<T> WhereMethodCall<T>(
            this IDocQuery<T> source, Expression<Func<T, bool>> expression, MethodCallExpression node)
        {
            if (IsDocumentGetTag(node) && (node.Type == typeof(bool) || node.Type == typeof(bool?)))
            {
                // This is a raw boolean field access like 'where doc.GetTag<bool>("foo")'
                 source.InnerQuery.WhereEqual(GetValue(node.Arguments[0]) as string, true);
                return source;
            }

            MethodInfo translatedMethod;
            if (functionMappings.TryGetValue(node.Method, out translatedMethod))
            {
                var objTransformed = new ObjectNormalizer().Visit(node.Object) as MethodCallExpression;
                if (!(IsDocumentGetTag(objTransformed) &&
                    objTransformed.Object == expression.Parameters[0]))
                {
                    throw new InvalidOperationException(
                      "The left-hand side of a supported function call must be a Document tag.");
                }
                var fieldPath = GetValue(objTransformed.Arguments[0]);
                var containedIn = GetValue(node.Arguments[0]);

                translatedMethod.Invoke(source.InnerQuery, new[] { fieldPath, containedIn });
                return source;
            }
           
            
            throw new InvalidOperationException(node.Method + " is not a supported method call in a where expression.");
        }
        private static bool IsDocumentGetTag(MethodCallExpression node)
        {
            if (node == null || node.Object == null)
            {
                return false;
            }
            if (!typeof(Document).IsAssignableFrom(node.Object.Type))
            {
                return false;
            }
            return node.Method.IsGenericMethod && node.Method.GetGenericMethodDefinition() == getMethod;
        }
        private static MethodInfo GetMethod<T>(Expression<Action<T>> expression)
        {
            return (expression.Body as MethodCallExpression).Method;
        }
    
        private static object GetValue(Expression exp)
        {
            try
            {
                return Expression.Lambda(
                    typeof(Func<>).MakeGenericType(exp.Type), exp).Compile().DynamicInvoke();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to evaluate expression: " + exp, e);
            }
        }
        class ObjectNormalizer : ExpressionVisitor
        {

           
            protected override Expression VisitMemberAccess(MemberExpression node)
            {
               
                return base.VisitMemberAccess(node);
            }

          
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var methodCall = Visit(node.Operand) as MethodCallExpression;
                if ((node.NodeType == ExpressionType.Convert ||
                    node.NodeType == ExpressionType.ConvertChecked) &&
                    IsDocumentGetTag(methodCall))
                {
                    return Expression.Call(methodCall.Object,
                        getMethod.MakeGenericMethod(node.Type),
                        methodCall.Arguments);
                }
                return base.VisitUnary(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
               
                return base.VisitMethodCall(node);
            }
        }
        /// <summary>
        /// Normalizes Where expressions.
        /// </summary>
        class WhereNormalizer : ExpressionVisitor
        {

           
            protected override Expression VisitBinary(BinaryExpression node)
            {
                var leftTransformed = new ObjectNormalizer().Visit(node.Left) as MethodCallExpression;
                var rightTransformed = new ObjectNormalizer().Visit(node.Right) as MethodCallExpression;

                MethodCallExpression objectExpression;
                Expression filterExpression;
                bool inverted;
                if (leftTransformed != null)
                {
                    objectExpression = leftTransformed;
                    filterExpression = node.Right;
                    inverted = false;
                }
                else {
                    objectExpression = rightTransformed;
                    filterExpression = node.Left;
                    inverted = true;
                }

                try
                {
                    switch (node.NodeType)
                    {
                        case ExpressionType.GreaterThan:
                            if (inverted)
                            {
                                return Expression.LessThan(objectExpression, filterExpression);
                            }
                            else {
                                return Expression.GreaterThan(objectExpression, filterExpression);
                            }
                        case ExpressionType.GreaterThanOrEqual:
                            if (inverted)
                            {
                                return Expression.LessThanOrEqual(objectExpression, filterExpression);
                            }
                            else {
                                return Expression.GreaterThanOrEqual(objectExpression, filterExpression);
                            }
                        case ExpressionType.LessThan:
                            if (inverted)
                            {
                                return Expression.GreaterThan(objectExpression, filterExpression);
                            }
                            else {
                                return Expression.LessThan(objectExpression, filterExpression);
                            }
                        case ExpressionType.LessThanOrEqual:
                            if (inverted)
                            {
                                return Expression.GreaterThanOrEqual(objectExpression, filterExpression);
                            }
                            else {
                                return Expression.LessThanOrEqual(objectExpression, filterExpression);
                            }
                        case ExpressionType.Equal:
                            return Expression.Equal(objectExpression, filterExpression);
                        case ExpressionType.NotEqual:
                            return Expression.NotEqual(objectExpression, filterExpression);
                    }
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("Operation not supported: " + node);
                }
                return base.VisitBinary(node);
            }

            /// <summary>
            /// If a ! operator is used, this removes the ! and instead calls the equivalent
            /// function (so e.g. == becomes !=, &lt; becomes &gt;=, Contains becomes NotContains)
            /// </summary>
            protected override Expression VisitUnary(UnaryExpression node)
            {
                // Normalizes inversion
                if (node.NodeType == ExpressionType.Not)
                {
                    var visitedOperand = Visit(node.Operand);
                    var binaryOperand = visitedOperand as BinaryExpression;
                    if (binaryOperand != null)
                    {
                        switch (binaryOperand.NodeType)
                        {
                            case ExpressionType.GreaterThan:
                                return Expression.LessThanOrEqual(binaryOperand.Left, binaryOperand.Right);
                            case ExpressionType.GreaterThanOrEqual:
                                return Expression.LessThan(binaryOperand.Left, binaryOperand.Right);
                            case ExpressionType.LessThan:
                                return Expression.GreaterThanOrEqual(binaryOperand.Left, binaryOperand.Right);
                            case ExpressionType.LessThanOrEqual:
                                return Expression.GreaterThan(binaryOperand.Left, binaryOperand.Right);
                            case ExpressionType.Equal:
                                return Expression.NotEqual(binaryOperand.Left, binaryOperand.Right);
                            case ExpressionType.NotEqual:
                                return Expression.Equal(binaryOperand.Left, binaryOperand.Right);
                        }
                    }

                    
                }
                return base.VisitUnary(node);
            }

            /// <summary>
            /// Normalizes .Equals into == and Contains() into the appropriate stub.
            /// </summary>
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                // Convert .Equals() into ==
                if (node.Method.Name == "Equals" &&
                    node.Method.ReturnType == typeof(bool) &&
                    node.Method.GetParameters().Length == 1)
                {
                    var obj = new ObjectNormalizer().Visit(node.Object) as MethodCallExpression;
                    var parameter = new ObjectNormalizer().Visit(node.Arguments[0]) as MethodCallExpression;
                    if ((IsDocumentGetTag(obj) && (obj.Object is ParameterExpression)) ||
                        (IsDocumentGetTag(parameter) && (parameter.Object is ParameterExpression)))
                    {
                        return Expression.Equal(node.Object, node.Arguments[0]);
                    }
                }

                return base.VisitMethodCall(node);
            }
        }

    }

}
