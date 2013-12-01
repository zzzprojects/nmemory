// ----------------------------------------------------------------------------------
// <copyright file="UnaryExpressionCloner.cs" company="NMemory Team">
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
// ---------------------------------------------------------------------------------

namespace NMemory.Common.Expressions
{
    using System.Linq.Expressions;
    using NMemory.Common.Visitors;

    internal class UnaryExpressionCloner : IExpressionBuilder
    {
        private readonly Expression root;
        private readonly Expression unary;

        private UnaryExpressionCloner(Expression original, Expression root)
        {
            this.unary = original;
            this.root = root; 
        }

        public static bool TryCreate(
            Expression unary, 
            Expression root, 
            out UnaryExpressionCloner result)
        {
            if (unary == null)
            {
                result = null;
                return false;
            }

            Expression expr = unary;

            do
            {
                // Check if we reached the root expression
                if (expr == root)
                {
                    result = new UnaryExpressionCloner(unary, root);
                    return true;
                }
            }
            while (TryGetNextStep(expr, out expr)) ;

            // Last chance: check for a default root node
            if (root == null && IsValidDefaultNode(expr))
            {
                result = new UnaryExpressionCloner(unary, expr);
                return true;
            }

            result = null;
            return false;
        }

        public Expression Build(Expression source)
        {
            return ReplaceVisitor.Replace(
                this.unary, 
                x => x == this.root, 
                x => source);
        }

        private static bool TryGetNextStep(Expression current, out Expression next)
        {
            UnaryExpression unary = current as UnaryExpression;

            if (unary != null)
            {
                next = unary.Operand;
                return true;
            }

            MemberExpression member = current as MemberExpression;

            if (member != null)
            {
                next = member.Expression;

                // The value of Expression if it represents a static member
                return next != null;
            }

            // Return the input
            next = current;
            return false;
        }

        private static bool IsValidDefaultNode(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Constant:
                //case ExpressionType.Parameter:
                    return true;
                default:
                    return false;
            }
        }
    }
}
