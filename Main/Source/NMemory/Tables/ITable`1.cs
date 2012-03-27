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
        /// <param name="originalEntity">An entity that contains the primary key of the entity to be updated.</param>
        /// <param name="entity">An entity that represents the new property values.</param>
        void Update(TEntity originalEntity, TEntity entity);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be deleted.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Occurs before a new entity is inserted.
        /// </summary>
        event EventHandler<EntityEventArgs<TEntity>> EntityInserting;

        /// <summary>
        /// Occurs after a new entity is inserted.
        /// </summary>
        event EventHandler<EntityEventArgs<TEntity>> EntityInserted;

        /// <summary>
        /// Occurs before an entity is updated.
        /// </summary>
        event EventHandler<EntityUpdateEventArgs<TEntity>> EntityUpdating;

        /// <summary>
        /// Occurs after an entity is updated.
        /// </summary>
        event EventHandler<EntityUpdateEventArgs<TEntity>> EntityUpdated;

        /// <summary>
        /// Occurs before an entity is deleted.
        /// </summary>
        event EventHandler<EntityEventArgs<TEntity>> EntityDeleting;

        /// <summary>
        /// Occurs after an entity is deleted.
        /// </summary>
        event EventHandler<EntityEventArgs<TEntity>> EntityDeleted;
    }
}
