using System;
using System.Linq.Expressions;
using NMemory.Indexes;
using NMemory.Tables;

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
            Func<TForeignKey, TPrimaryKey> convertForeignToPrimary = 
                RelationKeyConverterFactory.CreateForeignToPrimaryConverter(
                    primaryIndex.KeyInfo, 
                    foreignIndex.KeyInfo, 
                    constraints);

            Func<TPrimaryKey, TForeignKey> convertPrimaryToForeign =
                RelationKeyConverterFactory.CreatePrimaryToForeignConverter(
                    primaryIndex.KeyInfo,
                    foreignIndex.KeyInfo,
                    constraints);

            return tableCollection.CreateRelation(primaryIndex, foreignIndex, convertForeignToPrimary, convertPrimaryToForeign);
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

        public static ITable<T> FindTable<T>(this TableCollection tableCollection)
            where T : class
        {
            if (tableCollection == null)
	        {
		        throw new ArgumentNullException("tableCollection");
	        }

            return tableCollection.FindTable(typeof(T)) as ITable<T>;
        }
    }
}
