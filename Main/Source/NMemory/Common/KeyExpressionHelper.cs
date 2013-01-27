// ----------------------------------------------------------------------------------
// <copyright file="KeyExpressionHelper.cs" company="NMemory Team">
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

namespace NMemory.Common
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Indexes;

    internal static class KeyExpressionHelper
    {
        public static Expression<Func<TKey, bool>> CreateKeyEmptinessDetector<TEntity,TKey>(     
            IKeyInfoExpressionServices services)
        {
            ParameterExpression param = Expression.Parameter(typeof(TKey));

            Expression body = CreateKeyEmptinessDetector(param, services);

            return Expression.Lambda<Func<TKey, bool>>(body, param);
        }

        public static Expression CreateKeyEmptinessDetector(
            Expression source,
            IKeyInfoExpressionServices services)
        {
            Expression body = null;
            int memberCount = services.GetMemberCount();

            for (int i = 0; i < memberCount; i++)
            {
                Expression member = services.CreateKeyMemberSelectorExpression(source, i);
                Type memberType = member.Type;

                if (ReflectionHelper.IsNullable(memberType))
                {
                    Expression equalityTest =
                        Expression.Equal(
                            member,
                            Expression.Constant(null, memberType));

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

            // If all the members of the anonymous type is not nullable, the body expression 
            // is null at this point
            if (body == null)
            {
                body = Expression.Constant(false);
            }

            return body;
        }

        public static bool TryGetMemberMapping(
            MemberInfo[] left, 
            MemberInfo[] right,
            out int[] mapping)
        {
            if (left.Length != right.Length)
            {
                mapping = null;
                return false;
            }

            int length = left.Length;
            int[] result = new int[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = -1;

                for (int j = 0; j < length; j++)
                {
                    if (left[i] == right[j])
                    {
                        result[i] = j;
                        break;
                    }
                }

                if (result[i] == -1)
                {
                    mapping = null;
                    return false;
                }
            }

            mapping = result;
            return true;
        }

        public static Expression CreateKeyConversionExpression(
            Expression source,
            IKeyInfoExpressionServices from,
            IKeyInfoExpressionServices to,
            MemberInfo[] toMembers,
            int[] mapping)
        {
            int memberCount = mapping.Length;

            Expression[] factoryArgs = new Expression[memberCount];

            for (int i = 0; i < memberCount; i++)
            {
                Type requestedType = ReflectionHelper.GetMemberType(toMembers[i]);
                Expression arg = from.CreateKeyMemberSelectorExpression(source, mapping[i]);

                if (arg.Type != requestedType)
                {
                    arg = Expression.Convert(arg, requestedType);
                }

                factoryArgs[i] = arg;
            }

            return to.CreateKeyFactoryExpression(factoryArgs);
        }
    }
}
