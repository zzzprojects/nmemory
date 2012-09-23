using System.Linq;
using NMemory.Indexes;
using NMemory.Transactions;

namespace NMemory.Tables
{
    /// <summary>
    /// Defines an interface for database tables.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities, contained by the table.</typeparam>
    public interface ITable<TEntity> : ITable, IQueryable<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets the primary key index of the table.
        /// </summary>
        new IUniqueIndex<TEntity> PrimaryKeyIndex { get; }

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">An entity that represents the property values of the new entity.</param>
        void Insert(TEntity entity);

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">An entity that represents the property values of the new entity.</param>
        /// <param name="entity">The transaction within which the insert operation executes.</param>
        void Insert(TEntity entity, Transaction transaction);

        /// <summary>
        /// Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="entity">An entity that represents the new property values.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Updates the properties of the specified contained by the table.
        /// </summary>
        /// <param name="entity">An entity that represents the new property values.</param>
        /// <param name="entity">The transaction within which the update operation executes.</param>
        void Update(TEntity entity, Transaction transaction);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be deleted.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be deleted.</param>
        /// <param name="entity">The transaction within which the delete operation executes.</param>
        void Delete(TEntity entity, Transaction transaction);
    }
}
