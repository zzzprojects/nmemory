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
    using System.Linq;
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
        protected override Expression VisitMember(MemberExpression node)
        {
            // ----------------------------------------------------
            // original:
            //  dbParam.Table    OR
            //  dbConst.Table    OR
            //  const.db.Table
            // ----------------------------------------------------
            // transformed: 
            //  tableConstant
            // ----------------------------------------------------

            Type tableInterface = null;

            if (node.Type.IsInterface &&
                node.Type.IsGenericType && 
                node.Type.GetGenericTypeDefinition() == typeof(ITable<>))
            {
                tableInterface = node.Type;
            }
            else
            {
                tableInterface = node.Type
                    .GetInterfaces()
                    .FirstOrDefault(i => 
                        i.IsGenericType && 
                        i.GetGenericTypeDefinition() == typeof(ITable<>));
            }

            if (tableInterface == null)
            {
                return base.VisitMember(node);
            }

            Type entityType = tableInterface.GetGenericArguments()[0];

            IDatabase database = this.FindDatabase(node.Expression);

            if (database == null)
            {
                // TODO: This should not happen, log this
                return base.VisitMember(node);
            }

            ITable table = database.Tables.FindTable(entityType);

            return Expression.Constant(table);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // ----------------------------------------------------
            // original:
            //  executionContext.Database.Tables.FindTable<>   OR
            //  dbConst.Tables.FindTable<>                     OR
            //  const.db.Tables.FindTable<>
            // ----------------------------------------------------
            // transformed: 
            //  tableConstant
            // ----------------------------------------------------

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
            ConstantExpression constant = databaseAccess as ConstantExpression;

            if (constant != null)
            {
                return constant.Value as IDatabase;
            }

            MemberExpression member = databaseAccess as MemberExpression;

            if (member != null)
            {
                // ExecutionContext
                if (member.Member == DatabaseMembers.ExecutionContext_Database)
                {
                    return this.Database;
                }

                ConstantExpression source = member.Expression as ConstantExpression;

                if (source != null)
                {
                    return ReflectionHelper
                        .GetMemberValue(member.Member, source.Value) as IDatabase;
                }
            }

            return null;
        }
    }
}