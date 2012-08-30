using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;
using NMemory.Modularity;
using System.Linq.Expressions;

namespace NMemory.Tables
{
    /// <summary>
    /// Represents a collection of the tables of the database.
    /// </summary>
    public class TableCollection
    {
        private IDatabase database;
        private List<ITable> tables;
        private Dictionary<ITable, List<IRelation>> referredRelations;
        private Dictionary<ITable, List<IRelation>> referringRelations;

        private HashSet<Type> entityTypes;

        internal TableCollection(IDatabase database)
	    {
            this.database = database;
            this.tables = new List<ITable>();
            this.referredRelations = new Dictionary<ITable, List<IRelation>>();
            this.referringRelations = new Dictionary<ITable, List<IRelation>>();

            this.entityTypes = new HashSet<Type>();
	    }

        /// <summary>
        /// Returns all database tables.
        /// </summary>
        /// <returns>A list of the tables.</returns>
        public IList<ITable> GetAllTables()
        {
            return this.tables.AsReadOnly();
        }

        /// <summary>
        /// Initializes a database table.
        /// </summary>
        /// <typeparam name="TEntity">Specifies the type of the entities of the table.</typeparam>
        /// <typeparam name="TPrimaryKey">Specifies the type of the primary key of the entities.</typeparam>
        /// <param name="primaryKey">An expression to determine the primary key of the entities.</param>
        /// <param name="identitySpecification">An <c>IdentitySpecification</c> to set an identity field.</param>
        /// <param name="initialEntities">An <c>IEnumerable</c> to set the initial entities of the table.</param>
        /// <returns>An <c>Table</c> that represents the defined table object.</returns>
        public Table<TEntity, TPrimaryKey> Create<TEntity, TPrimaryKey>(

            Expression<Func<TEntity, TPrimaryKey>> primaryKey,
            IdentitySpecification<TEntity> identitySpecification = null,
            IEnumerable<TEntity> initialEntities = null)

            where TEntity : class
        {
            Table<TEntity, TPrimaryKey> table = this.database.DatabaseEngine.TableFactory.CreateTable<TEntity, TPrimaryKey>(
                primaryKey,
                identitySpecification,
                initialEntities);

            this.tables.Add(table);
            this.entityTypes.Add(table.EntityType);

            this.referredRelations.Add(table, new List<IRelation>());
            this.referringRelations.Add(table, new List<IRelation>());

            return table;
        }

        /// <summary>
        /// Creates a relation between two tables.
        /// </summary>
        /// <typeparam name="TPrimary">The type of the entities of the primary table.</typeparam>
        /// <typeparam name="TPrimaryKey">The type of the primary key of the entities of the primary table.</typeparam>
        /// <typeparam name="TForeign">The type of the entities of the foreign table.</typeparam>
        /// <typeparam name="TForeignKey">Type type of the foreign key of the foreign table.</typeparam>
        /// <param name="primaryIndex">An <c>IIndex</c> that specifies the primary key.</param>
        /// <param name="foreignIndex">An <c>IIndex</c> that specifies the foreign key.</param>
        /// <param name="convertForeignToPrimary">A function to convert a foreign key to the corresponding primary key.</param>
        /// <param name="convertPrimaryToForeign">A function to convert a primary key to the corresponding foreign key.</param>
        /// <returns></returns>
        public Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
                IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
                IIndex<TForeign, TForeignKey> foreignIndex,
                Func<TForeignKey, TPrimaryKey> convertForeignToPrimary,
                Func<TPrimaryKey, TForeignKey> convertPrimaryToForeign
            )
            where TPrimary : class
            where TForeign : class
        {
            
            var result = new Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
                primaryIndex,
                foreignIndex,
                convertForeignToPrimary,
                convertPrimaryToForeign);

            IRelation relation = result;

            // Validate the relation
            foreach (TForeign item in foreignIndex.SelectAll())
            {
                relation.ValidateEntity(item);
            }

            this.referringRelations[relation.PrimaryTable].Add(relation);
            this.referredRelations[relation.ForeignTable].Add(relation);

            return result;
        }

        public Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
                IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
                IIndex<TForeign, TForeignKey> foreignIndex,
                IRelationContraint[] constraints
            )
            where TPrimary : class
            where TForeign : class
        {
            throw new NotImplementedException();
        }

        internal IList<IRelation> GetReferringRelations(ITable primaryTable)
        {
            return this.referringRelations[primaryTable].AsReadOnly();
        }

        internal IList<IRelation> GetReferredRelations(ITable foreignTable)
        {
            return this.referredRelations[foreignTable].AsReadOnly();
        }

        internal IList<IRelation> GetReferringRelations(IIndex primaryIndex)
        {
            return this.referringRelations[primaryIndex.Table].Where(x => x.PrimaryIndex == primaryIndex).ToList();
        }

        internal IList<IRelation> GetReferedRelations(IIndex foreignIndex)
        {
            return this.referredRelations[foreignIndex.Table].Where(x => x.ForeignIndex == foreignIndex).ToList();
        }

        internal bool IsEntityType<T>()
        {
            return this.entityTypes.Contains(typeof(T));
        }

        internal ITable<T> FindTable<T>()
            where T : class
        {
            return this.tables.SingleOrDefault(t => t.EntityType == typeof(T)) as ITable<T>;
        }

    }
}
