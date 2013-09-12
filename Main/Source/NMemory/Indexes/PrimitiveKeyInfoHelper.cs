// ----------------------------------------------------------------------------------
// <copyright file="PrimitiveKeyInfoHelper.cs" company="NMemory Team">
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

namespace NMemory.Indexes
{
    using NMemory.Common;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class PrimitiveKeyInfoHelper : IKeyInfoHelper
    {
        private Type primitiveType;

        public PrimitiveKeyInfoHelper(Type primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        public int GetMemberCount()
        {
            return 1;
        }

        public Expression CreateKeyFactoryExpression(params Expression[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            if (arguments.Length != 1)
            {
                throw new ArgumentException("", "arguments");
            }

            if (arguments[0].Type != primitiveType)
            {
                throw new ArgumentException("", "arguments");
            }

            return arguments[0];
        }

        public Expression CreateKeyMemberSelectorExpression(Expression source, int index)
        {
            if (index != 0)
            {
                throw new ArgumentException("", "index");
            }

            return source;
        }

        public bool TryParseKeySelectorExpression(
            Expression keySelector,
            bool strict,
            out MemberInfo[] result)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            result = null;

            if (keySelector.Type != this.primitiveType)
            {
                return false;
            }

            Expression expr = keySelector;

            if (!strict)
            {
                expr = ExpressionHelper.SkipConversionNodes(expr);
            }

            MemberExpression member = expr as MemberExpression;

            if (member == null)
            {
                return false;
            }

            //if (member.Expression.NodeType != ExpressionType.Parameter)
            //{
            //    throw new ArgumentException("Invalid expression", "keySelector");
            //}

            result = new MemberInfo[] { member.Member };
            return true;
        }
    }
}
