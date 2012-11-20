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
            LambdaExpression collectionSelector = null;

            // Check if the method is SelectMany and get the collection selector
            if (!QueryExpressionHelper.GetSelectManyCollectionSelector(
                node, 
                out collectionSelector))
            {
                return base.VisitMethodCall(node);
            }

            // Unwrap the selector (hopefully Where expression)
            MethodCallExpression where = 
                ExpressionHelper.SkipConversionNodes(collectionSelector.Body) 
                    as MethodCallExpression;

            LambdaExpression predicate = null;

            // Check if the selector of SelectMany is Where and get the predicate
            if (!QueryExpressionHelper.GetWherePredicate(where, out predicate))
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
            // Result selector (same as the SeletMany result selector)
            return Expression.Call(
                joinMethod,
                outerSource,
                innerSource,
                Expression.Quote(outerKey),
                Expression.Quote(innerKey),
                node.Arguments[2]);
        }
    }
}
