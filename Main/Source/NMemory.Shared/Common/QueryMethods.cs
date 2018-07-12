// ----------------------------------------------------------------------------------
// <copyright file="QueryMethods.cs" company="NMemory Team">
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

namespace NMemory.Common
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Linq;

    internal class QueryMethods
    {
        public static MethodInfo SelectMany
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.SelectMany<object, object, object>(
                        null,
                        x => null,
                        (x, y) => null));
            }
        }

        public static MethodInfo First
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.First<object>(null));
            }
        }

        public static MethodInfo Select
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.Select<object, object>(
                        null,
                        x => null));
            }
        }

        public static MethodInfo Where
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.Where<object>(
                        null,
                        x => false));
            }
        }

        public static MethodInfo DefaultIfEmpty
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.DefaultIfEmpty<object>(
                        null));
            }
        }

        public static MethodInfo EnumerableDefaultIfEmpty
        {
            get
            {
                return GetMethodInfo(() =>
                    Enumerable.DefaultIfEmpty<object>(
                        null));
            }
        }

        public static MethodInfo SelectManyNoResultSelector
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.SelectMany<object, object>(
                        null,
                        x => null));
            }
        }

        public static MethodInfo Join
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.Join<object, object, object, object>(
                        null,
                        null,
                        x => null,
                        x => null,
                        (x, y) => null));
            }
        }

        public static MethodInfo JoinIndexed
        {
            get
            {
                return GetMethodInfo(() =>
                    EnumerableEx.JoinIndexed<object, object, object, object, object>(
                        null,
                        null,
                        x => null,
                        x => false,
                        x => null,
                        (x, y) => null));
            }
        }

        public static MethodInfo GroupJoin
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.GroupJoin<object, object, object, object>(
                        null,
                        null,
                        x => null,
                        x => null,
                        (x, y) => null));
            }
        }

        public static MethodInfo GroupJoinIndexed
        {
            get
            {
                return GetMethodInfo(() =>
                    EnumerableEx.GroupJoinIndexed<object, object, object, object, object>(
                        null,
                        null,
                        x => null,
                        x => false,
                        x => null,
                        (x, y) => null));
            }
        }

        public static MethodInfo AsQueryable
        {
            get
            {
                return GetMethodInfo(() =>
                    Queryable.AsQueryable<object>(null));
            }
        }

        private static MethodInfo GetMethodInfo(Expression<Func<object>> expression)
        {
            MethodCallExpression methodCall = expression.Body as MethodCallExpression;

            if (methodCall == null)
            {
                return null;
            }

            return methodCall.Method.GetGenericMethodDefinition();
        }
    }
}
