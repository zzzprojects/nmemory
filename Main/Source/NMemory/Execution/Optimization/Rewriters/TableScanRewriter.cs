// ----------------------------------------------------------------------------------
// <copyright file="TableScanRewriter.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;

    public class TableScanRewriter : ExpressionRewriterBase
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            // original:
            //
            //  DatabaseTable
            //
            // transformed:
            //  
            //  DatabaseTable.SelectAll<>

            Type entityType = DatabaseReflectionHelper.GetTableEntityType(node.Type);

            if (entityType != null)
            {
                return CreateSelectTableExpression(node, entityType);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // original:
            //
            //  dbParam.FindTable<>
            //
            // transformed:
            //  
            //  dbParam.FindTable<>.SelectAll<>

            Type entityType = DatabaseReflectionHelper.GetTableEntityType(node.Type);

            if (entityType != null && 
                node.Method.IsGenericMethod && 
                node.Method.GetGenericMethodDefinition() == DatabaseMembers.TableCollectionExtensions_FindTable)
            {
                return CreateSelectTableExpression(node, entityType);
            }

            return base.VisitMethodCall(node);
        }

        private static Expression CreateSelectTableExpression(Expression source, Type entityType)
        {
            MethodInfo selectAllMethod = DatabaseMembers.TableExtensions_SelectAll.MakeGenericMethod(entityType);

            Expression selectAllCall = Expression.Call(selectAllMethod, source);

            return Expression.Call(
                QueryMethods.AsQueryable.MakeGenericMethod(entityType),
                selectAllCall);
        }
    }
}
