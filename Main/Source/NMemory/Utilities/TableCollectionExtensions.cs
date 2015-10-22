// -----------------------------------------------------------------------------------
// <copyright file="TableCollectionExtensions.cs" company="NMemory Team">
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
// -----------------------------------------------------------------------------------

namespace NMemory.Utilities
{
    using System;
    using System.Linq.Expressions;
    using NMemory.Indexes;
    using NMemory.Tables;

    public static class TableCollectionExtensions
    {
        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
            this TableCollection tableCollection, 
            IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex, 
            IIndex<TForeign, TForeignKey> foreignIndex,
            RelationOptions options,
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

            return tableCollection.CreateRelation(primaryIndex, foreignIndex, convertForeignToPrimary, convertPrimaryToForeign, options);
        }

        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
            this TableCollection tableCollection,
            IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
            IIndex<TForeign, TForeignKey> foreignIndex,
            params IRelationContraint[] constraints)

            where TPrimary : class
            where TForeign : class
        {
            return tableCollection.CreateRelation(
                primaryIndex, 
                foreignIndex,
                new RelationOptions(),
                constraints);
        }

        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey, TConstraint1>(
          this TableCollection tableCollection,
          IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
          IIndex<TForeign, TForeignKey> foreignIndex,
          Expression<Func<TPrimary, TConstraint1>> constraint1P, Expression<Func<TForeign, TConstraint1>> constraint1F,
          RelationOptions options = null)

            where TPrimary : class
            where TForeign : class
        {
            return CreateRelation(
                tableCollection, 
                primaryIndex, 
                foreignIndex,
                options,
                new RelationConstraint<TPrimary, TForeign, TConstraint1>(constraint1P, constraint1F));
        }

        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey, TConstraint1, TConstraint2>(
           this TableCollection tableCollection,
           IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
           IIndex<TForeign, TForeignKey> foreignIndex,
           Expression<Func<TPrimary, TConstraint1>> constraint1P, Expression<Func<TForeign, TConstraint1>> constraint1F,
           Expression<Func<TPrimary, TConstraint2>> constraint2P, Expression<Func<TForeign, TConstraint2>> constraint2F,
           RelationOptions options = null)

            where TPrimary : class
            where TForeign : class
        {
            return CreateRelation(
                tableCollection, 
                primaryIndex, 
                foreignIndex,
                options,
                new RelationConstraint<TPrimary, TForeign, TConstraint1>(constraint1P, constraint1F),
                new RelationConstraint<TPrimary, TForeign, TConstraint2>(constraint2P, constraint2F));
        }

        public static Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey, TConstraint1, TConstraint2, TContraint3>(
           this TableCollection tableCollection,
           IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
           IIndex<TForeign, TForeignKey> foreignIndex,
           Expression<Func<TPrimary, TConstraint1>> constraint1P, Expression<Func<TForeign, TConstraint1>> constraint1F,
           Expression<Func<TPrimary, TConstraint2>> constraint2P, Expression<Func<TForeign, TConstraint2>> constraint2F,
           Expression<Func<TPrimary, TConstraint2>> constraint3P, Expression<Func<TForeign, TConstraint2>> constraint3F,
           RelationOptions options = null)

            where TPrimary : class
            where TForeign : class
        {
            return CreateRelation(
                tableCollection, 
                primaryIndex, 
                foreignIndex,
                options,
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
