// ----------------------------------------------------------------------------------
// <copyright file="AnonymousTypeKeyInfo`2.cs" company="NMemory Team">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;

    public class AnonymousTypeKeyInfo<TEntity, TKey> : KeyInfoBase<TEntity, TKey>, IKeyInfoExpressionBuilderProvider
        where TEntity : class
    {
        public AnonymousTypeKeyInfo(Expression<Func<TEntity, TKey>> keySelector, SortOrder[] sortOrders) : 
            base(
                GetEntityKeyMembers(keySelector),
                sortOrders,
                new AnonymousTypeKeyComparer<TKey>(sortOrders),
                keySelector.Compile(),
                CreateKeyEmptinessDetector())
        {
        }

        public AnonymousTypeKeyInfo(Expression<Func<TEntity, TKey>> keySelector) :
            this(keySelector, GetDefaultSortOrders(keySelector))
        {
        }

        IKeyInfoExpressionBuilder IKeyInfoExpressionBuilderProvider.KeyInfoExpressionBuilder
        {
            get
            {
                return new AnonymousTypeKeyInfoExpressionBuilder(typeof(TKey));
            }
        }

        private static SortOrder[] GetDefaultSortOrders(Expression<Func<TEntity, TKey>> keySelector)
        {
            int fieldCount = GetEntityKeyMembers(keySelector).Length;

            return Enumerable.Repeat(SortOrder.Ascending, fieldCount).ToArray();
        }

        private static MemberInfo[] GetEntityKeyMembers(Expression<Func<TEntity, TKey>> keySelector)
        {
            NewExpression resultCreator = keySelector.Body as NewExpression;

            if (resultCreator == null)
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            if (!ReflectionHelper.IsAnonymousType(resultCreator.Type))
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            List<MemberInfo> result = new List<MemberInfo>();

            foreach (Expression arg in resultCreator.Arguments)
            {
                MemberExpression member = arg as MemberExpression;

                if (member == null)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }

                if (member.Expression.NodeType != ExpressionType.Parameter)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }

                result.Add(member.Member);
            }

            return result.ToArray();
        }

        private static Func<TKey, bool> CreateKeyEmptinessDetector()
        {
            Type keyType = typeof(TKey);
            PropertyInfo[] properties = keyType.GetProperties();

            ParameterExpression keyParam = Expression.Parameter(keyType);
            Expression body = null;

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                Type propertyType = property.PropertyType;

                if (ReflectionHelper.IsNullable(propertyType))
                {
                    Expression equalityTest =
                        Expression.Equal(
                            Expression.Property(keyParam, property),
                            Expression.Constant(null, propertyType));

                    if (body == null)
                    {
                        body = equalityTest;
                    }
                    else
                    {
                        body = Expression.Or(body, equalityTest);
                    }
                }
            }

            // If all the members of the anonymous type is not nullable, the body expression is null at this point
            if (body == null)
            {
                body = Expression.Constant(false);
            }

            Expression<Func<TKey, bool>> resultExpression =
                Expression.Lambda<Func<TKey, bool>>(body, keyParam);

            return resultExpression.Compile();
        }
    }
}
