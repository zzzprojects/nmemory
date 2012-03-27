using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;

namespace NMemory.Tables
{
    /// <summary>
    /// Represents a collection of the tables of the database.
    /// </summary>
    public class TableCollection
    {
        private List<ITable> tables;
        private Dictionary<ITable, List<IRelation>> referredRelations;
        private Dictionary<ITable, List<IRelation>> referringRelations;

        private HashSet<Type> entityTypes;

        internal TableCollection()
	    {
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

        internal void RegisterTable(ITable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException();
            }

            this.tables.Add(table);
            this.entityTypes.Add(table.EntityType);

            this.referredRelations.Add(table, new List<IRelation>());
            this.referringRelations.Add(table, new List<IRelation>());
        }

        internal IList<IRelation> GetReferringRelations(ITable primaryTable)
        {
            return this.referringRelations[primaryTable].AsReadOnly();
        }

        internal IList<IRelation> GetReferredRelations(ITable foreignTable)
        {
            return this.referredRelations[foreignTable].AsReadOnly();
        }

        internal IList<IRelation> GetReferedRelations(IIndex foreignIndex)
        {
            return this.referredRelations[foreignIndex.Table].Where(x => x.ForeignIndex == foreignIndex).ToList();
        }

        internal void RegisterRelation(IRelation relation)
        {
            this.referringRelations[relation.PrimaryTable].Add(relation);
            this.referredRelations[relation.ForeignTable].Add(relation);
        }

        internal bool IsEntityType<T>()
        {
            return this.entityTypes.Contains(typeof(T));
        }

    }
}
