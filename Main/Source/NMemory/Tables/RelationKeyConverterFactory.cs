// -----------------------------------------------------------------------------------
// <copyright file="RelationKeyConverterFactory.cs" company="NMemory Team">
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
// -----------------------------------------------------------------------------------

namespace NMemory.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Indexes;

    public static class RelationKeyConverterFactory
    {
        public static Func<TPrimaryKey, TForeignKey> CreatePrimaryToForeignConverter<TPrimaryKey, TForeignKey>(
            IKeyInfo<TPrimaryKey> primaryKey,
            IKeyInfo<TForeignKey> foreignKey,
            params IRelationContraint[] constraints)
        {
            return CreateConversion(primaryKey, foreignKey, CreateMapping(primaryKey, foreignKey, constraints, false));
        }

        public static Func<TForeignKey, TPrimaryKey> CreateForeignToPrimaryConverter<TPrimaryKey, TForeignKey>(
            IKeyInfo<TPrimaryKey> primaryKey,
            IKeyInfo<TForeignKey> foreignKey,
            params IRelationContraint[] constraints)
        {
            return CreateConversion(foreignKey, primaryKey, CreateMapping(primaryKey, foreignKey, constraints, true));
        }

        private static Func<TFrom, TTo> CreateConversion<TFrom, TTo>(
            IKeyInfo<TFrom> fromKeyInfo,
            IKeyInfo<TTo> toKeyInfo,
            int[] mapping)
        {
            IKeyInfoExpressionBuilder fromKeyInfoExprBuilder = GetExpressionBuilder(fromKeyInfo);
            IKeyInfoExpressionBuilder toKeyInfoExprBuilder = GetExpressionBuilder(toKeyInfo);

            int memberCount = mapping.Length;

            ParameterExpression keyParam = Expression.Parameter(fromKeyInfo.KeyType);
            Expression[] factoryArgs = new Expression[memberCount];

            for (int i = 0; i < memberCount; i++)
            {
                Type requestedType = ReflectionHelper.GetMemberUnderlyingType(toKeyInfo.EntityKeyMembers[i]);
                Expression arg = fromKeyInfoExprBuilder.CreateKeyMemberSelectorExpression(keyParam, mapping[i]);

                if (arg.Type != requestedType)
                {
                    arg = Expression.Convert(arg, requestedType);
                }

                factoryArgs[i] = arg;
            }

            Expression factory = toKeyInfoExprBuilder.CreateKeyFactoryExpression(factoryArgs);

            return Expression.Lambda<Func<TFrom, TTo>>(factory, keyParam).Compile();
        }

        private static int[] CreateMapping<TFrom, TTo>(IKeyInfo<TFrom> primaryKeyInfo, IKeyInfo<TTo> foreignKeyInfo, IRelationContraint[] constraints, bool invert)
        {
            int memberCount = primaryKeyInfo.EntityKeyMembers.Length;

            if (foreignKeyInfo.EntityKeyMembers.Length != memberCount)
            {
                throw new ArgumentException("", "foreignKeyInfo");
            }

            if (constraints.Length != memberCount)
            {
                throw new ArgumentException("", "constraints");
            }

            List<int> mapping = new List<int>();

            for (int i = 0; i < memberCount; i++)
            {
                MemberInfo fromMember = primaryKeyInfo.EntityKeyMembers[i];

                IRelationContraint constraint = constraints.FirstOrDefault(c => c.PrimaryField == fromMember);

                if (constraint == null)
                {
                    throw new ArgumentException("", "constraints");
                }

                int mappedIndex = foreignKeyInfo.EntityKeyMembers.ToList().IndexOf(constraint.ForeignField);

                if (mappedIndex == -1)
                {
                    throw new ArgumentException("", "constraints");
                }

                mapping.Add(mappedIndex);
            }

            if (invert)
            {
                List<int> invertedMapping = new List<int>();

                for (int i = 0; i < memberCount; i++)
                {
                    invertedMapping.Add(mapping.IndexOf(i));
                }
     
                mapping = invertedMapping;
            }

            return mapping.ToArray();
        }

        private static IKeyInfoExpressionBuilder GetExpressionBuilder(IKeyInfo keyInfo)
        {
            IKeyInfoExpressionBuilderProvider builderProvider = null;

            builderProvider = keyInfo as IKeyInfoExpressionBuilderProvider;

            if (builderProvider == null)
            {
                throw new ArgumentException("", "keyInfo");
            }

            IKeyInfoExpressionBuilder keyInfoExprBuilder = builderProvider.KeyInfoExpressionBuilder;

            if (keyInfoExprBuilder == null)
            {
                throw new ArgumentException("", "keyInfo");
            }

            return keyInfoExprBuilder;
        }
    }
}
