// ----------------------------------------------------------------------------------
// <copyright file="GroupJoinPhysicalRewriter.cs" company="NMemory Team">
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
    using NMemory.Common;
    using NMemory.Indexes;
    using NMemory.Tables;
    using System.Linq.Expressions;

    public class GroupJoinPhysicalRewriter : ExpressionRewriterBase
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.Method.IsGenericMethod ||
                node.Method.GetGenericMethodDefinition() != QueryMethods.GroupJoin)
            {
                // Not GroupJoin
                return base.VisitMethodCall(node);
            }

            ConstantExpression secondExpression = node.Arguments[1] as ConstantExpression;

            if (secondExpression == null)
            {
                // Not constant expression
                return base.VisitMethodCall(node);
            }

            ITable table = secondExpression.Value as ITable;

            if (table == null)
            {
                // Not table
                return base.VisitMethodCall(node);
            }

            Expression secondKeySelector = node.Arguments[3];

       
            foreach (IIndex index in table.Indexes)
            {
                var keys = index.KeyInfo.EntityKeyMembers;

                var rels = this.Database.Tables.GetReferringRelations(index);

            }

            return base.VisitMethodCall(node);
        }
    }
}
