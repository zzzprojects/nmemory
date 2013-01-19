// ----------------------------------------------------------------------------------
// <copyright file="EqualityMappingDetector.cs" company="NMemory Team">
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


namespace NMemory.Execution.Optimization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;

    public class EqualityMappingDetector : ExpressionVisitor
    {
        private IList<MemberChain> leftMembers;
        private IList<MemberChain> rightMembers;

        private Expression leftSource;
        private Expression rightSource;

        public EqualityMappingDetector(Expression left, Expression right)
        {
            this.leftSource = left;
            this.rightSource = right;
            this.leftMembers = new List<MemberChain>();
            this.rightMembers = new List<MemberChain>();
        }

        public MemberChain[] LeftMembers
        {
            get
            {
                return this.leftMembers.ToArray();
            }
        }

        public MemberChain[] RightMembers
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

                // Detect the operands of the AND operation
                // Check if the sides referred to members of different sources
                return 
                    this.DetectAndOperand(equal.Left) && 
                    this.DetectAndOperand(equal.Right) &&
                    this.leftMembers.Count == this.rightMembers.Count;
            }

            if (expression.NodeType != ExpressionType.AndAlso)
            {
                return false;
            }

            BinaryExpression binary = expression as BinaryExpression;

            return this.Detect(binary.Left) && this.Detect(binary.Right);
        }

        private bool DetectAndOperand(Expression expression)
        {
            expression = ExpressionHelper.SkipConversionNodes(expression);

            MemberExpression member = expression as MemberExpression;

            if (member == null)
            {
                return false;
            }

            return
                this.DetectMember(member, this.leftSource, this.leftMembers) ||
                this.DetectMember(member, this.rightSource, this.rightMembers);

        }

        private bool DetectMember(
            MemberExpression member, 
            Expression source,
            IList<MemberChain> memberChainCollector)
        {
            List<MemberInfo> memberChain = new List<MemberInfo>();

            do
            {
                memberChain.Add(member.Member);

                if (member.Expression == source)
                {
                    memberChainCollector.Add(new MemberChain(memberChain));
                    return true;
                }

                member = member.Expression as MemberExpression;
            }
            while (member != null);

            return false;
        }
    }
}
