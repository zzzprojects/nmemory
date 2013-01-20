// ----------------------------------------------------------------------------------
// <copyright file="DatabaseParameterRewriter.cs" company="NMemory Team">
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
    using NMemory.Common;
    using NMemory.Modularity;

    public class DatabaseParameterRewriter : ExpressionRewriterBase
    {
        private ITransformationContext context;

        public DatabaseParameterRewriter(ITransformationContext context)
        {
            this.context = context;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // ----------------------------------------------------
            // original:
            //  dbParam.DatabaseTable
            // ----------------------------------------------------
            // transformed: 
            //  executionContextParam.Database.Tables.FindTable<T>
            // ----------------------------------------------------

            if (typeof(IDatabase).IsAssignableFrom(node.Member.DeclaringType) &&
                node.Expression is ParameterExpression)
            {
                Type entityType = DatabaseReflectionHelper.GetTableEntityType(node.Type);

                // Check if ITable<>
                if (entityType != null)
                {
                    Expression database = Expression.Property(this.context.Parameter, DatabaseMembers.ExecutionContext_Database);
                    Expression tables = Expression.Property(database, DatabaseMembers.Database_Tables);

                    return Expression.Call(DatabaseMembers.TableCollectionExtensions_FindTable.MakeGenericMethod(entityType), tables);
                }
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // ----------------------------------------------------
            // original:
            //  dbParam.Database
            // ----------------------------------------------------
            // transformed:
            //  executionContextParam.Database
            // ----------------------------------------------------

            if (typeof(IDatabase).IsAssignableFrom(node.Type))
            {
                return Expression.Property(this.context.Parameter, DatabaseMembers.ExecutionContext_Database);
            }

            return base.VisitParameter(node);
        }
    }
}
