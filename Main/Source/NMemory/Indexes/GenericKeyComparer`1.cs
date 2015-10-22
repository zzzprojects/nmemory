// ----------------------------------------------------------------------------------
// <copyright file="GenericKeyComparer`1.cs" company="NMemory Team">
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
// ---------------------------------------------------------------------------------

namespace NMemory.Indexes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Exceptions;

    public class GenericKeyComparer<T> 
        : IComparer<T>
    {
        private Func<T, T, int> comparer;

        public GenericKeyComparer(SortOrder[] sortOrders, IKeyInfoHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException("helper");
            }

            int memberCount = helper.GetMemberCount();

            if (memberCount <= 0)
            {
                throw new ArgumentException(
                    ExceptionMessages.InvalidKeyInfoHelper,
                    "helper");
            }

            if (sortOrders == null)
            {
                sortOrders = Enumerable.Repeat(SortOrder.Ascending, memberCount).ToArray();
            }

            if (sortOrders.Length != memberCount)
            {
                throw new ArgumentException(
                    ExceptionMessages.MemberAndSortOrderCountMismatch,
                    "sortOrders");
            }

            ParameterExpression x = Expression.Parameter(typeof(T), "x");
            ParameterExpression y = Expression.Parameter(typeof(T), "y");

            ParameterExpression variable = Expression.Variable(typeof(int), "var");

            List<Expression> blockBody = new List<Expression>();

            for (int i = 0; i < memberCount; i++)
            {
                Expression xMember = helper.CreateKeyMemberSelectorExpression(x, i);
                Expression yMember = helper.CreateKeyMemberSelectorExpression(y, i);

                Expression expr =
                    Expression.Assign(
                            variable,
                            this.CreateComparsion(xMember, yMember, sortOrders[i]));

                if (0 < i)
                {
                     expr = 
                         Expression.IfThen(
                            Expression.Equal(variable, Expression.Constant(0)),
                            expr);
                }

                blockBody.Add(expr);
            }

            // Eval the last variable
            blockBody.Add(variable);

            var lambda = Expression.Lambda<Func<T, T, int>>(
                Expression.Block(
                    new ParameterExpression[] { variable },
                    blockBody),
                x,
                y);

            this.comparer = lambda.Compile();
        }

        public int Compare(T x, T y)
        {
            if (x == null)
            {
                throw new ArgumentException(ExceptionMessages.GenericKeyInfoCannotBeNull, "x");
            }

            if (y == null)
            {
                throw new ArgumentException(ExceptionMessages.GenericKeyInfoCannotBeNull, "y");
            }

            int result = this.comparer(x, y);

            return result;
        }

        private Expression CreateComparsion(Expression x, Expression y, SortOrder ordering)
        {
            MethodInfo compareMethod =
                ReflectionHelper.GetStaticMethodInfo(() =>
                    PrimitiveKeyComparer.Compare<object>(null, null, SortOrder.Ascending));

            compareMethod = compareMethod
                .GetGenericMethodDefinition()
                .MakeGenericMethod(x.Type);

            return Expression.Call(
                compareMethod,
                x,
                y,
                Expression.Constant(ordering, typeof(SortOrder)));
        }
    }
}
