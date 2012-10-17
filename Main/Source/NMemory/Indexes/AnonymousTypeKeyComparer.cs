// ----------------------------------------------------------------------------------
// <copyright file="AnonymousTypeKeyComparer.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;

    internal class AnonymousTypeKeyComparer<T> : IComparer<T>
    {
        private Func<T, T, int> comparer;

        public AnonymousTypeKeyComparer(SortOrder[] sortOrders)
        {
            var marker = typeof(T).GetCustomAttributes(typeof(DebuggerDisplayAttribute), false).Cast<DebuggerDisplayAttribute>();

            if (marker.All(m => m.Type != "<Anonymous Type>"))
            {
                throw new InvalidOperationException("The specified generic type is not an anonymous type");
            }

            PropertyInfo[] properties = typeof(T).GetProperties();

            if (properties.Length == 0)
            {
                throw new InvalidOperationException("No properties");
            }

            if (sortOrders == null)
            {
                sortOrders = Enumerable.Repeat(SortOrder.Ascending, properties.Length).ToArray();
            }

            if (sortOrders.Length != properties.Length)
            {
                throw new ArgumentException("The count of sort ordering values does not match the count of anonymous type propeties", "sortOrders");
            }

            ParameterExpression x = Expression.Parameter(typeof(T), "x");
            ParameterExpression y = Expression.Parameter(typeof(T), "y");

            ParameterExpression variable = Expression.Variable(typeof(int), "var");

            List<Expression> blockBody = new List<Expression>();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo p = properties[i];

                if (i == 0)
                {
                    blockBody.Add(
                        Expression.Assign(
                            variable, 
                            this.CreateComparsion(p, x, y, sortOrders[i])));
                }
                else
                {
                    blockBody.Add(
                        Expression.IfThen(
                            Expression.Equal(variable, Expression.Constant(0)),
                            Expression.Assign(variable, this.CreateComparsion(p, x, y, sortOrders[i]))));
                }
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
                throw new ArgumentException("Anonymous type key cannot be null", "x");
            }

            if (y == null)
            {
                throw new ArgumentException("Anonymous type key cannot be null", "y");
            }

            int result = this.comparer(x, y);

            return result;
        }

        private Expression CreateComparsion(PropertyInfo p, ParameterExpression x, ParameterExpression y, SortOrder ordering)
        {
            MethodInfo compareMethod =
                ReflectionHelper.GetStaticMethodInfo(() =>
                    PrimitiveKeyComparer.Compare<object>(null, null, SortOrder.Ascending));

            compareMethod = compareMethod.GetGenericMethodDefinition().MakeGenericMethod(p.PropertyType);

            return Expression.Call(
                compareMethod,
                Expression.Property(x, p),
                Expression.Property(y, p),
                Expression.Constant(ordering, typeof(SortOrder)));
        }
    }
}
