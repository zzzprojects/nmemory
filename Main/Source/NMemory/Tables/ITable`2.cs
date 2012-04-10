using NMemory.Indexes;

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
        /// Updates the properties of an entity contained by the table.
        /// </summary>
        /// <param name="key">The primary key of the entity.</param>
        /// <param name="entity">An entity that represents the new property values.</param>
        void Update(TPrimaryKey key, TEntity entity);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="key">The primary key of the entity to be deleted.</param>
        void Delete(TPrimaryKey key);
    }
}
