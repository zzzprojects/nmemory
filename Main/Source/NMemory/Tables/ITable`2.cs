using NMemory.Indexes;
using NMemory.Transactions;

namespace NMemory.Tables
{
    /// <summary>
    /// Defines an interface for database tables.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities, contained by the table.</typeparam>
    /// <typeparam name="TPrimaryKey">The type of the primary key of the entities of the table.</typeparam>
    public interface ITable<TEntity, TPrimaryKey> : ITable<TEntity>
    {
        /// <summary>
        /// Gets the primary key index of the table.
        /// </summary>
        new IUniqueIndex<TEntity, TPrimaryKey> PrimaryKeyIndex { get; }

        /// <summary>
        /// Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="key">The primary key of the entity.</param>
        /// <param name="entity">An entity that represents the new property values.</param>
        void Update(TPrimaryKey key, TEntity entity);

        /// <summary>
        /// Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="key">The primary key of the entity.</param>
        /// <param name="entity">An entity that represents the new property values.</param>
        /// <param name="entity">The transaction within which the update operation executes.</param>
        void Update(TPrimaryKey key, TEntity entity, Transaction transaction);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="key">The primary key of the entity to be deleted.</param>
        void Delete(TPrimaryKey key);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="key">The primary key of the entity to be deleted.</param>
        /// <param name="entity">The transaction within which the update operation executes.</param>
        void Delete(TPrimaryKey key, Transaction transaction);


    }
}
