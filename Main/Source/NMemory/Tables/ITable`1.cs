using System;
using System.Linq;
using NMemory.Indexes;

namespace NMemory.Tables
{
    /// <summary>
    /// Defines an interface for a database tables.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities, contained by the table.</typeparam>
    public interface ITable<TEntity> : ITable, IQueryable<TEntity>
    {
        /// <summary>
        /// Gets the primary key index of the table.
        /// </summary>
        new IIndex<TEntity> PrimaryKeyIndex { get; }

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">An entity that represents the property values of the new entity.</param>
        void Insert(TEntity entity);

        /// <summary>
        /// Updates the properties of an entity contained by the table.
        /// </summary>
        /// <param name="entity">An entity that represents the new property values.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be deleted.</param>
        void Delete(TEntity entity);
    }
}
