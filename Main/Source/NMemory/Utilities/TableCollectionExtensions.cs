using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Indexes;
using System.Reflection;
using System.Linq.Expressions;
using NMemory.Common;

namespace NMemory.Utilities
{
    public static class TableCollectionExtensions
    {
        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
            this TableCollection tableCollection, 
            IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex, 
            IIndex<TForeign, TForeignKey> foreignIndex, 
            params IRelationContraint[] constraints)

            where TPrimary : class
            where TForeign : class
        {
            if (tableCollection == null)
            {
                throw new ArgumentNullException("tableCollection");
            }

            if (primaryIndex == null)
            {
                throw new ArgumentNullException("primaryIndex");
            }

            if (foreignIndex == null)
            {
                throw new ArgumentNullException("foreignIndex");
            }

            if (constraints == null)
            {
                throw new ArgumentNullException("constaints");
            }

            IKeyInfo primaryKey = primaryIndex.KeyInfo;
            IKeyInfo foreignKey = foreignIndex.KeyInfo;

            IKeyInfoExpressionBuilderProvider builderProvider = null;
            IKeyInfoExpressionBuilder primaryKeyInfoExprBuilder = null;
            IKeyInfoExpressionBuilder foreignKeyInfoExprBuilder = null;

            builderProvider = primaryKey as IKeyInfoExpressionBuilderProvider;

            if (builderProvider == null)
            {
                throw new ArgumentException("", "primaryIndex");
            }

            primaryKeyInfoExprBuilder = builderProvider.KeyInfoExpressionBuilder;

            if (primaryKeyInfoExprBuilder == null)
            {
                throw new ArgumentException("", "primaryIndex");
            }

            builderProvider = foreignKey as IKeyInfoExpressionBuilderProvider;

            if (builderProvider == null)
            {
                throw new ArgumentException("", "foreignIndex");
            }

            foreignKeyInfoExprBuilder = builderProvider.KeyInfoExpressionBuilder;

            if (foreignKeyInfoExprBuilder == null)
            {
                throw new ArgumentException("", "foreignIndex");
            }

            // Builders are prepared

            int memberCount = primaryKey.EntityKeyMembers.Length;

            if (foreignKey.EntityKeyMembers.Length != memberCount)
            {
                throw new ArgumentException("", "foreignIndex");
            }

            if (constraints.Length != memberCount)
            {
                throw new ArgumentException("", "constraints");
            }

            List<int> primaryToForeignMapping = new List<int>();

            for (int i = 0; i < memberCount; i++)
            {
                MemberInfo primaryMember = primaryKey.EntityKeyMembers[i];

                IRelationContraint constraint = constraints.FirstOrDefault(c => c.PrimaryField == primaryMember);

                if (constraint == null)
                {
                    throw new ArgumentException("", "constraints");
                }

                int mappedIndex = foreignKey.EntityKeyMembers.ToList().IndexOf(constraint.ForeignField);

                if (mappedIndex == -1)
                {
                    throw new ArgumentException("", "constraints");
                }

                primaryToForeignMapping.Add(mappedIndex);
            }

            Func<TForeignKey, TPrimaryKey> convertforeignToPrimary = null;
            Func<TPrimaryKey, TForeignKey> convertPrimaryToForeign = null;

            // Create Foreign to Primary converter
            {
                ParameterExpression foreignKeyParam = Expression.Parameter(foreignKey.KeyType);
                Expression[] factoryArgs = new Expression[memberCount];

                for (int i = 0; i < memberCount; i++)
                {
                    Type requestedType = ReflectionHelper.GetMemberUnderlyingType(primaryKey.EntityKeyMembers[i]);
                    Expression arg = foreignKeyInfoExprBuilder.CreateKeyMemberSelectorExpression(foreignKeyParam, primaryToForeignMapping[i]);

                    if (arg.Type != requestedType)
                    {
                        arg = Expression.Convert(arg, requestedType);
                    }

                    factoryArgs[i] = arg;
                }

                Expression factory = primaryKeyInfoExprBuilder.CreateKeyFactoryExpression(factoryArgs);

                convertforeignToPrimary = Expression.Lambda<Func<TForeignKey, TPrimaryKey>>(factory, foreignKeyParam).Compile();
            }

            // Create Primary to Foreign converter
            {
                ParameterExpression primaryKeyParam = Expression.Parameter(primaryKey.KeyType);
                Expression[] factoryArgs = new Expression[memberCount];

                for (int i = 0; i < memberCount; i++)
                {
                    Type requestedType = ReflectionHelper.GetMemberUnderlyingType(foreignKey.EntityKeyMembers[i]);
                    Expression arg = primaryKeyInfoExprBuilder.CreateKeyMemberSelectorExpression(primaryKeyParam, primaryToForeignMapping[i]);

                    if (arg.Type != requestedType)
                    {
                        arg = Expression.Convert(arg, requestedType);
                    }

                    factoryArgs[i] = arg;
                }

                Expression factory = foreignKeyInfoExprBuilder.CreateKeyFactoryExpression(factoryArgs);

                convertPrimaryToForeign = Expression.Lambda<Func<TPrimaryKey, TForeignKey>>(factory, primaryKeyParam).Compile();
            }

            return tableCollection.CreateRelation(primaryIndex, foreignIndex, convertforeignToPrimary, convertPrimaryToForeign);
        }

        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey, TConstraint1>(
          this TableCollection tableCollection,
          IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
          IIndex<TForeign, TForeignKey> foreignIndex,
          Expression<Func<TPrimary, TConstraint1>> constraint1P, Expression<Func<TForeign, TConstraint1>> constraint1F)

            where TPrimary : class
            where TForeign : class
        {
            return CreateRelation(tableCollection, primaryIndex, foreignIndex,
                new RelationConstraint<TPrimary, TForeign, TConstraint1>(constraint1P, constraint1F));
        }

        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey, TConstraint1, TConstraint2>(
           this TableCollection tableCollection,
           IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
           IIndex<TForeign, TForeignKey> foreignIndex,
           Expression<Func<TPrimary, TConstraint1>> constraint1P, Expression<Func<TForeign, TConstraint1>> constraint1F,
           Expression<Func<TPrimary, TConstraint2>> constraint2P, Expression<Func<TForeign, TConstraint2>> constraint2F)

            where TPrimary : class
            where TForeign : class
        {
            return CreateRelation(tableCollection, primaryIndex, foreignIndex,
                new RelationConstraint<TPrimary, TForeign, TConstraint1>(constraint1P, constraint1F),
                new RelationConstraint<TPrimary, TForeign, TConstraint2>(constraint2P, constraint2F));
        }

        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey, TConstraint1, TConstraint2, TContraint3>(
           this TableCollection tableCollection,
           IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
           IIndex<TForeign, TForeignKey> foreignIndex,
           Expression<Func<TPrimary, TConstraint1>> constraint1P, Expression<Func<TForeign, TConstraint1>> constraint1F,
           Expression<Func<TPrimary, TConstraint2>> constraint2P, Expression<Func<TForeign, TConstraint2>> constraint2F,
           Expression<Func<TPrimary, TConstraint2>> constraint3P, Expression<Func<TForeign, TConstraint2>> constraint3F)

            where TPrimary : class
            where TForeign : class
        {
            return CreateRelation(tableCollection, primaryIndex, foreignIndex,
                new RelationConstraint<TPrimary, TForeign, TConstraint1>(constraint1P, constraint1F),
                new RelationConstraint<TPrimary, TForeign, TConstraint2>(constraint2P, constraint2F),
                new RelationConstraint<TPrimary, TForeign, TConstraint2>(constraint3P, constraint3F));
        }
    }
}
