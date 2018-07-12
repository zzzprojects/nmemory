// ----------------------------------------------------------------------------------
// <copyright file="EqualityMappingDetector.cs" company="NMemory Team">
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


namespace NMemory.Execution.Optimization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Common.Expressions;

    public class EqualityMappingDetector : ExpressionVisitor
    {
        private IList<IExpressionBuilder> leftMembers;
        private IList<IExpressionBuilder> rightMembers;

        private Expression leftSource;
        private Expression rightSource;

        public EqualityMappingDetector(Expression left, Expression right)
        {
            this.leftSource = left;
            this.rightSource = right;
            this.leftMembers = new List<IExpressionBuilder>();
            this.rightMembers = new List<IExpressionBuilder>();
        }

        public IExpressionBuilder[] LeftMembers
        {
            get
            {
                return this.leftMembers.ToArray();
            }
        }

        public IExpressionBuilder[] RightMembers
        {
            get
            {
                return this.rightMembers.ToArray();
            }
        }

        public bool Detect(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Equal)
            {
                BinaryExpression equal = expression as BinaryExpression;

                // Detect the operands of the EQUAL operation
                return 
                    this.DetectEqualOperand(equal.Left) && 
                    this.DetectEqualOperand(equal.Right) &&
                    this.leftMembers.Count == this.rightMembers.Count;
            }

            if (expression.NodeType != ExpressionType.AndAlso)
            {
                return false;
            }

            // 'expression' is an AndAlso expression
            BinaryExpression binary = expression as BinaryExpression;

            return this.Detect(binary.Left) && this.Detect(binary.Right);
        }

        private bool DetectEqualOperand(Expression expression)
        {
            return 
                DetectEqualOperand(expression, this.leftSource, this.leftMembers) ||
                DetectEqualOperand(expression, this.rightSource, this.rightMembers);
        }

        private bool DetectEqualOperand(
            Expression expression, 
            Expression source,
            IList<IExpressionBuilder> collector)
        {
            UnaryExpressionCloner expander = null;

            if (!UnaryExpressionCloner.TryCreate(expression, source, out expander))
            {
                return false;
            }

            collector.Add(expander);
            return true;
        }
    }
}
