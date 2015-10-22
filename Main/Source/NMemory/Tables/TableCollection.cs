// -----------------------------------------------------------------------------------
// <copyright file="TableCollection.cs" company="NMemory Team">
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

namespace NMemory.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Exceptions;
    using NMemory.Indexes;
    using NMemory.Modularity;
    using NMemory.Services.Contracts;

    /// <summary>
    ///     Represents a collection of the tables of the database.
    /// </summary>
    public class TableCollection
    {
        private readonly IDatabase database;
        private readonly List<ITable> tables;
        private readonly List<IRelation> relations;

        private readonly Dictionary<ITable, RelationGroup> relationMapping;

        private HashSet<Type> entityTypes;

        internal TableCollection(IDatabase database)
        {
            this.database = database;
            this.tables = new List<ITable>();
            this.relations = new List<IRelation>();

            this.relationMapping = new Dictionary<ITable, RelationGroup>();

            this.entityTypes = new HashSet<Type>();
        }

        /// <summary>
        ///     Returns all database tables.
        /// </summary>
        /// <returns> A list of the tables. </returns>
        public IList<ITable> GetAllTables()
        {
            return this.tables.AsReadOnly();
        }

        /// <summary>
        ///     Returns all database table relations.
        /// </summary>
        /// <returns> A list of the table relations. </returns>
        public IList<IRelation> GetAllRelations()
        {
            return this.relations.AsReadOnly();
        }

        /// <summary>
        ///     Initializes a database table.
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Specifies the type of the entities of the table.
        /// </typeparam>
        /// <typeparam name="TPrimaryKey">
        ///     Specifies the type of the primary key of the entities.
        /// </typeparam>
        /// <param name="primaryKey">
        ///     An expression that represents the primary key of the entities.
        /// </param>
        /// <param name="identitySpecification">
        ///     An IdentitySpecification to set an identity field.
        /// </param>
        /// <returns>
        ///     An Table that represents the defined table object.
        /// </returns>
        public Table<TEntity, TPrimaryKey> Create<TEntity, TPrimaryKey>(
            Expression<Func<TEntity, TPrimaryKey>> primaryKey,
            IdentitySpecification<TEntity> identitySpecification)

            where TEntity : class
        {
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }

            IKeyInfoFactory keyInfoFactory = new ModularKeyInfoFactory(this.database);

            return this.Create(keyInfoFactory.Create(primaryKey), identitySpecification);
        }

        /// <summary>
        ///     Initializes a database table.
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Specifies the type of the entities of the table.
        ///  </typeparam>
        /// <typeparam name="TPrimaryKey">
        ///     Specifies the type of the primary key of the entities.
        ///  </typeparam>
        /// <param name="primaryKey">
        ///     An IKeyInfo object that represents the primary key of the entities.
        /// </param>
        /// <param name="identitySpecification">
        ///     An IdentitySpecification to set an identity field.
        /// </param>
        /// <returns>
        ///     The table.
        /// </returns>
        public Table<TEntity, TPrimaryKey> Create<TEntity, TPrimaryKey>(
            IKeyInfo<TEntity, TPrimaryKey> primaryKey,
            IdentitySpecification<TEntity> identitySpecification)

            where TEntity : class
        {
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }

            ITableService service = this.database
                .DatabaseEngine
                .ServiceProvider
                .GetService<ITableService>();

            if (service == null)
            {
                throw new NMemoryException(ExceptionMessages.ServiceNotFound, "TableService");
            }

            Table<TEntity, TPrimaryKey> table = 
                service.CreateTable<TEntity, TPrimaryKey>(
                    primaryKey,
                    identitySpecification,
                    this.database);

            if (table == null)
            {
                throw new NMemoryException(ExceptionMessages.TableNotCreated);  
            }

            this.RegisterTable(table);

            return table;
        }

        /// <summary>
        ///     Creates a relation between two tables.
        /// </summary>
        /// <typeparam name="TPrimary">
        ///     The type of the entities of the primary table.
        /// </typeparam>
        /// <typeparam name="TPrimaryKey">
        ///     The type of the primary key of the entities of the primary table.
        /// </typeparam>
        /// <typeparam name="TForeign">
        ///     The type of the entities of the foreign table.
        /// </typeparam>
        /// <typeparam name="TForeignKey">
        ///     Type type of the foreign key of the foreign table.
        /// </typeparam>
        /// <param name="primaryIndex">
        ///     An IIndex that specifies the primary key.
        /// </param>
        /// <param name="foreignIndex">
        ///     An IIndex that specifies the foreign key.
        /// </param>
        /// <param name="convertForeignToPrimary">
        ///     A function to convert a foreign key to the corresponding primary key.
        /// </param>
        /// <param name="convertPrimaryToForeign">
        ///     A function to convert a primary key to the corresponding foreign key.
        /// </param>
        /// <returns>
        ///     The relation.
        /// </returns>
        public Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
                IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
                IIndex<TForeign, TForeignKey> foreignIndex,
                Func<TForeignKey, TPrimaryKey> convertForeignToPrimary,
                Func<TPrimaryKey, TForeignKey> convertPrimaryToForeign,
                RelationOptions relationOptions
            )
            where TPrimary : class
            where TForeign : class
        {
            if (primaryIndex == null)
            {
                throw new ArgumentNullException("primaryIndex");
            }

            if (foreignIndex == null)
            {
                throw new ArgumentNullException("foreignIndex");
            }

            if (convertForeignToPrimary == null)
            {
                throw new ArgumentNullException("convertForeignToPrimary");
            }

            if (convertPrimaryToForeign == null)
            {
                throw new ArgumentNullException("convertPrimaryToForeign");
            }

            if (relationOptions == null)
            {
                // Use default relation options
                relationOptions = new RelationOptions();
            }

            var result = new Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
                primaryIndex,
                foreignIndex,
                convertForeignToPrimary,
                convertPrimaryToForeign,
                relationOptions);

            IRelationInternal relation = result;

            relation.ValidateAll();

            this.relationMapping[relation.PrimaryTable].Referring.Add(relation);
            this.relationMapping[relation.ForeignTable].Referred.Add(relation);
            this.relations.Add(relation);

            return result;
        }

        public ITable FindTable(Type entityType)
        {
            return this.tables.SingleOrDefault(t => t.EntityType == entityType);
        }

        internal IList<IRelationInternal> GetReferringRelations(ITable primaryTable)
        {
            return this.relationMapping[primaryTable].ReferringReadOnly;
        }

        internal IList<IRelationInternal> GetReferredRelations(ITable foreignTable)
        {
            return this.relationMapping[foreignTable].ReferredReadOnly;
        }

        internal IList<IRelationInternal> GetReferringRelations(IIndex primaryIndex)
        {
            return this.GetReferringRelations(primaryIndex.Table)
                .Where(x => x.PrimaryIndex == primaryIndex)
                .ToList();
        }

        internal IList<IRelationInternal> GetReferredRelations(IIndex foreignIndex)
        {
            return this.GetReferredRelations(foreignIndex.Table)
                .Where(x => x.ForeignIndex == foreignIndex)
                .ToList();
        }

        internal bool IsEntityType<T>()
        {
            return this.entityTypes.Contains(typeof(T));
        }

        private void RegisterTable(ITable table)
        {
            foreach (ITableCatalog catalog in 
                this.database.DatabaseEngine.Components.OfType<ITableCatalog>())
            {
                catalog.RegisterTable(table);
            }

            this.tables.Add(table);
            this.entityTypes.Add(table.EntityType);

            this.relationMapping.Add(table, new RelationGroup());
        }
    }
}
