// ----------------------------------------------------------------------------------
// <copyright file="TableScanRewriter.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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
    using NMemory.Tables;
    using NMemory.Modularity;

    /// <summary>
    /// Rewrites all kind of table access expression to constant expression.
    /// </summary>
    public class TableAccessRewriter : ExpressionRewriterBase
    {
        public TableAccessRewriter(ITransformationContext context)
        {
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.Method.IsGenericMethod ||
                node.Method.GetGenericMethodDefinition() != DatabaseMembers.TableCollectionExtensions_FindTable)
            {
                return base.VisitMethodCall(node);
            }

            Type entityType = DatabaseReflectionHelper.GetTableEntityType(node.Type);

            if (entityType == null)
            {
                // TODO: This should not happen, log this
                return base.VisitMethodCall(node);
            }

            MemberExpression tablesProp = node.Arguments[0] as MemberExpression;

            if (tablesProp == null || tablesProp.Member != DatabaseMembers.Database_Tables)
            {
                return base.VisitMethodCall(node);
            }

            IDatabase database = this.FindDatabase(tablesProp.Expression);

            if (database == null)
            {
                // TODO: This should not happen, log this
                return base.VisitMethodCall(node);
            }

            ITable table = database.Tables.FindTable(entityType);

            return Expression.Constant(table);
        }

        private IDatabase FindDatabase(Expression databaseAccess)
        {
            ConstantExpression databaseConstant = databaseAccess as ConstantExpression;

            if (databaseConstant != null)
            {
                return databaseConstant.Value as IDatabase;
            }

            MemberExpression databaseContextMember = databaseAccess as MemberExpression;

            if (databaseContextMember != null &&
                databaseContextMember.Member == DatabaseMembers.ExecutionContext_Database)
            {
                return this.Database;
            }

            return null;
        }
    }
}