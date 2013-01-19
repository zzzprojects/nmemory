// ----------------------------------------------------------------------------------
// <copyright file="PropertyAccessRewriter.cs" company="NMemory Team">
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

    public class PropertyAccessRewriter : ExpressionRewriterBase
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (!ReflectionHelper.IsNullable(node.Type))
            {
                return base.VisitMember(node);
            }

            return this.CreateSafeMemberAccessExpression(node, node.Type);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType != ExpressionType.Convert && node.NodeType != ExpressionType.ConvertChecked)
            {
                return base.VisitUnary(node);
            }

            if (node.Operand.NodeType != ExpressionType.MemberAccess)
            {
                return base.VisitUnary(node);
            }

            if (!ReflectionHelper.IsNullable(node.Type))
            {
                return base.VisitUnary(node);
            }

            return this.CreateSafeMemberAccessExpression(node.Operand as MemberExpression, node.Type);
        }

        private Expression CreateSafeMemberAccessExpression(MemberExpression memberNode, Type resultType)
        {
            // ----------------------------------------------------
            // original:
            //  x.Member
            // ----------------------------------------------------
            // transformed:
            //// {
            ////    var source = x;
            ////    var result;
            ////
            ////    if (source == null)
            ////       result = source.Member;
            ////    else
            ////       result = null
            ////
            ////    return result;
            //// }
            //// --------------------------------------------------

            Expression[] blockContent = new Expression[3];

            ParameterExpression source = Expression.Variable(memberNode.Expression.Type);
            ParameterExpression result = Expression.Variable(resultType);

            Expression evaluation = Expression.PropertyOrField(source, memberNode.Member.Name);

            if (evaluation.Type != resultType)
            {
                evaluation = Expression.ConvertChecked(evaluation, resultType);
            }

            blockContent[0] = Expression.Assign(source, this.Visit(memberNode.Expression));
            blockContent[1] = Expression.IfThenElse(
                Expression.Equal(source, Expression.Constant(null, source.Type)),
                Expression.Assign(result, Expression.Constant(null, resultType)),
                Expression.Assign(result, evaluation));
            blockContent[2] = result;

            return Expression.Block(resultType, new ParameterExpression[] { source, result }, blockContent);
        }
    }
}
