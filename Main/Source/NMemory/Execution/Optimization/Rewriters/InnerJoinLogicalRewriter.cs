// ----------------------------------------------------------------------------------
// <copyright file="InnerJoinLogicalRewriter.cs" company="NMemory Team">
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
    /// Searches for SelectMany based inner join calls and replaces it with Join call
    /// </summary>
    public class InnerJoinLogicalRewriter : ExpressionRewriterBase
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // Check if SelectMany
            if (node.Method.DeclaringType != typeof(Queryable) ||
                !node.Method.IsGenericMethod ||
                node.Method.GetGenericMethodDefinition() != QueryMethods.SelectMany)
            {
                // Not SelectMany call
                return base.VisitMethodCall(node);
            }

            LambdaExpression collectionSelector = 
                ExpressionHelper.SkipQuoteNode(node.Arguments[1]) as LambdaExpression;

            if (collectionSelector == null)
            {
                return base.VisitMethodCall(node);
            }

            // Check if the selector of SelectMany is Where
            MethodCallExpression node2 = 
                ExpressionHelper.SkipConversionNodes(collectionSelector.Body) 
                    as MethodCallExpression;

            if (node2.Method.DeclaringType != typeof(Queryable) ||
                !node2.Method.IsGenericMethod ||
                node2.Method.GetGenericMethodDefinition() != QueryMethods.Where)
            {
                // Where method was not found
                return base.VisitMethodCall(node);
            }

            // Check if the source of the Where expression contains any parameter reference
            // The inner selector of the Join statement does not have parameter, so if the
            // source refers it, it cannot be used
            bool parameterReferred = ExpressionSearchVisitor.Search(
                node2.Arguments[0],
                collectionSelector.Parameters[0]);

            if (parameterReferred)
            {
                return base.VisitMethodCall(node);
            }

            // Get the predicate expression
            LambdaExpression predicate = 
                ExpressionHelper.SkipQuoteNode(node2.Arguments[1]) as LambdaExpression;

            // Analyse the predicate
            EqualityMappingDetector detector =
                new EqualityMappingDetector(
                    collectionSelector.Parameters[0],
                    predicate.Parameters[0]);

            if (!detector.Detect(predicate.Body))
            {
                // Cannot convert the predicate into join
                return base.VisitMethodCall(node);
            }

            // Everything is appropriate for a conversion, visit the inner and outer branches
            Expression outerSource = this.Visit(node.Arguments[0]);
            Expression innerSource = this.Visit(node2.Arguments[0]);

            // Build the keys
            Type outerSourceType = collectionSelector.Parameters[0].Type;
            Type innerSourceType = predicate.Parameters[0].Type;

            // Key selector parameters
            ParameterExpression outerKeyParam = 
                Expression.Parameter(outerSourceType, "outer");
            ParameterExpression innerKeyParam = 
                Expression.Parameter(innerSourceType, "inner");
            
            // Key members
            Expression[] outerKeyMemberAccess;
            Expression[] innerKeyMemberAccess;

            // Create key member accessor expressions
            CreateKeyMemberAccess(
                outerKeyParam,
                innerKeyParam,
                detector.LeftMembers,
                detector.RightMembers,
                out outerKeyMemberAccess,
                out innerKeyMemberAccess);

            // Build key selector lambdas
            LambdaExpression outerKey = 
                ExpressionHelper.CreateMemberSelectorLambdaExpression(
                    outerKeyParam, 
                    outerKeyMemberAccess);

            LambdaExpression innerKey =
                ExpressionHelper.CreateMemberSelectorLambdaExpression(
                    innerKeyParam, 
                    innerKeyMemberAccess);
            
            // Create the Join method definition:
            // It has the same generic type arguments as the SelectMany
            // Only add the key type (third argument)
            MethodInfo joinMethod = QueryMethods.Join.MakeGenericMethod(
                node.Method.GetGenericArguments()[0],
                node.Method.GetGenericArguments()[1],
                outerKey.Body.Type,
                node.Method.GetGenericArguments()[2]);

            // Create the Join call expression
            // Arguments:
            // Outer collection (visited source of SelectMany expression)
            // Inner collection (visited source of the Where expression)
            // Outer key selector
            // Inner key selector
            // Result selector (same as the )
            return Expression.Call(
                joinMethod,
                outerSource,
                innerSource,
                Expression.Quote(outerKey),
                Expression.Quote(innerKey),
                node.Arguments[2]);
        }

        private static void CreateKeyMemberAccess(
            Expression outerSource,
            Expression innerSource,
            MemberChain[] outerMembers,
            MemberChain[] innerMembers,
            out Expression[] outerResult,
            out Expression[] innerResult)
        {
            List<Expression> outer = new List<Expression>();
            List<Expression> inner = new List<Expression>();

            for (int i = 0; i < outerMembers.Length; i++)
            {
                Expression outerAccess = outerMembers[i].CreateExpression(outerSource);
                Expression innerAccess = innerMembers[i].CreateExpression(innerSource);

                ExpressionHelper.TryUnifyValueTypes(ref outerAccess, ref innerAccess);

                outer.Add(outerAccess);
                inner.Add(innerAccess);
            }

            outerResult = outer.ToArray();
            innerResult = inner.ToArray();
        }
    }
}
