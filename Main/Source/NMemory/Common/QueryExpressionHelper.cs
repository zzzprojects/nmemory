// ----------------------------------------------------------------------------------
// <copyright file="QueryExpressionHelper.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
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

namespace NMemory.Common
{
    using System.Linq;
    using System.Linq.Expressions;

    internal static class QueryExpressionHelper
    {
        public static bool GetSelectManyCollectionSelector(
            Expression node,
            out LambdaExpression collectionSelector)
        {
            collectionSelector = null;
            MethodCallExpression method = node as MethodCallExpression;

            if (method == null)
            {
                return false;
            }

            // Check if SelectMany
            if (method.Method.DeclaringType != typeof(Queryable) ||
                !method.Method.IsGenericMethod ||
                method.Method.GetGenericMethodDefinition() != QueryMethods.SelectMany)
            {
                // Not SelectMany call
                return false;
            }

            collectionSelector =
                ExpressionHelper.SkipQuoteNode(method.Arguments[1]) as LambdaExpression;

            return (collectionSelector != null);
        }

        public static bool GetWherePredicate(
            Expression node,
            out LambdaExpression predicate)
        {
            predicate = null;
            MethodCallExpression method = node as MethodCallExpression;

            if (method == null)
            {
                return false;
            }

            if (method.Method.DeclaringType != typeof(Queryable) ||
               !method.Method.IsGenericMethod ||
               method.Method.GetGenericMethodDefinition() != QueryMethods.Where)
            {
                // Where method was not found
                return false;
            }

            predicate =
                ExpressionHelper.SkipQuoteNode(method.Arguments[1]) as LambdaExpression;

            return (predicate != null);
        }

        public static bool GetDefaultOrEmptySource(
            Expression node,
            out Expression source)
        {
            source = null;
            MethodCallExpression method = node as MethodCallExpression;

            if (method == null)
            {
                return false;
            }

            if (method.Method.DeclaringType != typeof(Queryable) ||
               !method.Method.IsGenericMethod ||
               method.Method.GetGenericMethodDefinition() != QueryMethods.DefaultIfEmpty)
            {
                // Where method was not found
                return false;
            }

            source = method.Arguments[0];

            return (source != null);
        }
    }
}
