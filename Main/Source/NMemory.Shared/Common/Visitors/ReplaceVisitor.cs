// ----------------------------------------------------------------------------------
// <copyright file="ReplaceVisitor.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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

namespace NMemory.Common.Visitors
{
    using System;
    using System.Linq.Expressions;

    internal class ReplaceVisitor : ExpressionVisitor
    {
        private Func<Expression, bool> condition;
        private Func<Expression, Expression> converter;

        public ReplaceVisitor(
            Func<Expression, bool> condition, 
            Func<Expression, Expression> modifier)
        {
            this.condition = condition;
            this.converter = modifier;
        }

        public override Expression Visit(Expression expression)
        {
            if (this.condition.Invoke(expression))
            {
                return this.converter.Invoke(expression);
            }
            else
            {
                return base.Visit(expression);
            }
        }

        public static Expression Replace(
            Expression expression,
            Func<Expression, bool> condition,
            Func<Expression, Expression> modifier)
        {
            ReplaceVisitor visitor = new ReplaceVisitor(condition, modifier);

            return visitor.Visit(expression);
        }
    }
}
