using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common;

namespace NMemory.Tables
{
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
