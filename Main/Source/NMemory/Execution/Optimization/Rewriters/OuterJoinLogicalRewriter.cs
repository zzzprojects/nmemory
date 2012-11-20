// ----------------------------------------------------------------------------------
// <copyright file="OuterJoinLogicalRewriter.cs" company="NMemory Team">
//     Copyright (C) 2012 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.Execution.Optimization.Rewriters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Common.Visitors;

    /// <summary>
    /// Searches for SelectMany based outer join calls and replaces it with GroupJoin call
    /// </summary>
    public class OuterJoinLogicalRewriter : ExpressionRewriterBase
    {
        private static string JoinGroupInner;
        private static string JoinGroupOuter;
        private static MethodInfo JoinGroupCreate;

        static OuterJoinLogicalRewriter()
        {
            JoinGroupOuter =
                ReflectionHelper.GetMemberName((JoinGroup<object, object> g) => g.Outer);

            JoinGroupInner =
                ReflectionHelper.GetMemberName((JoinGroup<object, object> g) => g.Inner);

            JoinGroupCreate =
                ReflectionHelper.GetStaticMethodInfo(() =>
                    JoinGroup.Create<object, object>(null, null))
                    .GetGenericMethodDefinition();
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression expr = null;
            LambdaExpression collectionSelector = null;

            // Check if the method is SelectMany and get the collection selector
            if (!QueryExpressionHelper.GetSelectManyCollectionSelector(
                node, 
                out collectionSelector))
            {
                return base.VisitMethodCall(node);
            }

            MethodCallExpression defaultIfEmpty = 
                ExpressionHelper.SkipConversionNodes(collectionSelector.Body) 
                    as MethodCallExpression;

            // Check if the collection selector of the SelectMany is DefaultOrEmpty and
            // get its source
            if (!QueryExpressionHelper.GetDefaultOrEmptySource(defaultIfEmpty, out expr))
            {
                return base.VisitMethodCall(node);
            }

            MethodCallExpression where = expr as MethodCallExpression;
            LambdaExpression predicate = null;

            // Check if the source of DefaultOrEmpty is Where and get its predicate
            if (!QueryExpressionHelper.GetWherePredicate(where, out predicate))
            {
                return base.VisitMethodCall(node);
            }

            // Get the result selector of the original SelectMany expression.
            // It is used to achieve the exact same return type as the original SelectMany
            // expression has.
            LambdaExpression originalResultSelector =
                ExpressionHelper.SkipQuoteNode(node.Arguments[2]) as LambdaExpression;

            if (originalResultSelector == null)
            {
                return base.VisitMethodCall(node);
            }

            // Check if the source of the Where expression contains any parameter reference
            // The inner selector of the Join statement does not have parameter, so if the
            // source refers the parameter somewhere, it cannot be used as source
            bool parameterReferred = ExpressionSearchVisitor.Search(
                where.Arguments[0],
                collectionSelector.Parameters[0]);

            if (parameterReferred)
            {
                return base.VisitMethodCall(node);
            }

            // Analyse the predicate, find key mapping
            EqualityMappingDetector detector =
                new EqualityMappingDetector(
                    collectionSelector.Parameters[0],
                    predicate.Parameters[0]);

            if (!detector.Detect(predicate.Body))
            {
                // Cannot convert the predicate into join
                return base.VisitMethodCall(node);
            }

            // Everything is appropriate for a conversion, visit the inner and outer sources
            Expression outerSource = this.Visit(node.Arguments[0]);
            Expression innerSource = this.Visit(where.Arguments[0]);

            // Inner and outer entity types            
            Type outerType = collectionSelector.Parameters[0].Type;
            Type innerType = predicate.Parameters[0].Type;

            // Build the keys
            LambdaExpression outerKey;
            LambdaExpression innerKey;

            LinqJoinKeyHelper.CreateKeySelectors(
                outerType, 
                innerType,
                detector.LeftMembers,
                detector.RightMembers, 
                out outerKey, 
                out innerKey);

            // Create the result selector of the GroupJoin
            LambdaExpression groupJoinResultSelector =
                CreateGroupJoinResultSelector(outerType, innerType);

            Type groupJoinResultType = groupJoinResultSelector.Body.Type;

            // Create the GroupJoin method definition
            // Generic arguments:
            // Outer entity type (same as in SelectMany)
            // Inner entity type (same as in SelectMany)
            // Key type
            // Result type (custom tuple)
            MethodInfo groupJoinMethod = 
                QueryMethods.GroupJoin.MakeGenericMethod(
                    node.Method.GetGenericArguments()[0],
                    node.Method.GetGenericArguments()[1],
                    outerKey.Body.Type,
                    groupJoinResultType);

            // Create the Join call expression
            // Arguments:
            // Outer collection (visited source of SelectMany expression)
            // Inner collection (visited source of the Where expression)
            // Outer key selector
            // Inner key selector
            // Result selector (tuple)
            Expression groupJoin = Expression.Call(
                groupJoinMethod,
                outerSource,
                innerSource,
                Expression.Quote(outerKey),
                Expression.Quote(innerKey),
                Expression.Quote(groupJoinResultSelector));

            // Collection selector for the post SelectMany expression
            LambdaExpression postSelectManyCollectionSelector = 
                CreatePostSelectManyCollectionSelector(
                    innerType, 
                    groupJoinResultType);

            // Result selector for the post SelectMany expression.
            LambdaExpression postSelectManyResultSelector =
                CreatePostSelectManyResultSelector(
                    innerType,
                    groupJoinResultType,
                    originalResultSelector);

            // Define the method of the post SelectMany.
            // Generic arguments:
            // Result type of the GroupJoin
            // Inner entity type
            // Result type (same as the original SelectMany)
            MethodInfo postSelectManyMethod = 
                QueryMethods.SelectMany.MakeGenericMethod(
                    groupJoinResultType,
                    node.Method.GetGenericArguments()[1],
                    node.Method.GetGenericArguments()[2]);

            Expression postSelectMany =
                Expression.Call(
                    postSelectManyMethod,
                    groupJoin,
                    Expression.Quote(postSelectManyCollectionSelector),
                    Expression.Quote(postSelectManyResultSelector));

            return postSelectMany;
        }

        private static LambdaExpression CreateGroupJoinResultSelector(
            Type outerType,
            Type innerType)
        {
            // Lambda parameters
            ParameterExpression outerParameter =
                Expression.Parameter(
                    outerType, "outer");
            ParameterExpression innerParameter =
                Expression.Parameter(
                    typeof(IEnumerable<>).MakeGenericType(innerType), 
                    "inner");

            return Expression.Lambda(
                Expression.Call(
                    JoinGroupCreate.MakeGenericMethod(outerType, innerType),
                    outerParameter,
                    innerParameter),
                outerParameter,
                innerParameter);
        }

        private static LambdaExpression CreatePostSelectManyCollectionSelector(
            Type innerType, 
            Type groupJoinResultType)
        {
            PropertyInfo innerElements =
                groupJoinResultType.GetProperty(JoinGroupInner);
            ParameterExpression groupJoinResult = 
                Expression.Parameter(groupJoinResultType, "group");

            return Expression.Lambda(
                Expression.Call(
                    QueryMethods.EnumerableDefaultIfEmpty.MakeGenericMethod(innerType),
                    Expression.Property(groupJoinResult, innerElements)),
                groupJoinResult);
        }

        private static LambdaExpression CreatePostSelectManyResultSelector(
            Type innerType, 
            Type groupJoinResultType, 
            LambdaExpression originalResultSelector)
        {
            ParameterExpression groupJoinResult = 
                Expression.Parameter(groupJoinResultType, "group");
            ParameterExpression inner = 
                Expression.Parameter(innerType, "inner");

            // JoinGroup<,>.Outer property
            PropertyInfo outerElement = 
                groupJoinResultType.GetProperty(JoinGroupOuter);

            Expression expression = originalResultSelector.Body;

            // Change the first parameter
            expression = ReplaceVisitor.Replace(
                expression,
                e => e == originalResultSelector.Parameters[0],
                e => Expression.Property(groupJoinResult, outerElement));

            // Change the second parameter
            expression = ReplaceVisitor.Replace(
                expression,
                e => e == originalResultSelector.Parameters[1],
                e => inner);

            return Expression.Lambda(expression, groupJoinResult, inner);
        }
    }
}
