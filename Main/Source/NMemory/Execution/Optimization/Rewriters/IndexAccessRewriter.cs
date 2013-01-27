// ----------------------------------------------------------------------------------
// <copyright file="IndexAccessRewriter.cs" company="NMemory Team">
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
    using NMemory.Common;
    using NMemory.Indexes;

    /// <summary>
    /// Rewrites constant->member index access expression to constant expression.
    /// </summary>
    public class IndexAccessRewriter : ExpressionRewriterBase
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            // ----------------------------------------------------
            // original:
            //  const.index
            // ----------------------------------------------------
            // transformed: 
            //  indexConstant
            // ----------------------------------------------------

            Type indexInterface = null;

            if (node.Type.IsInterface &&
                node.Type.GetGenericTypeDefinition() == typeof(IIndex<>))
            {
                indexInterface = node.Type;
            }
            else
            {
                indexInterface = node.Type
                    .GetInterfaces()
                    .FirstOrDefault(i => 
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IIndex<>));
            }

            if (indexInterface == null)
            {
                return base.VisitMember(node);
            }

            ConstantExpression source = node.Expression as ConstantExpression;

            if (source == null)
            {
                return base.VisitMember(node);
            }

            return Expression.Constant(
                ReflectionHelper.GetMemberValue(node.Member, source.Value));
        }
    }
}