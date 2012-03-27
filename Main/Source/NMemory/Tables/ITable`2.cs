using NMemory.Indexes;

namespace NMemory.Tables
{
    /// <summary>
    /// Defines an interface for a database tables.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities, contained by the table.</typeparam>
    /// <typeparam name="TPrimaryKey">The type of the primary key of the entities of the table.</typeparam>
    public interface ITable<TEntity, TPrimaryKey> : ITable<TEntity>
    {
        /// <summary>
        /// Gets the primary key index of the table.
        /// </summary>
        new IUniqueIndex<TEntity, TPrimaryKey> PrimaryKeyIndex { get; }
    }
}
