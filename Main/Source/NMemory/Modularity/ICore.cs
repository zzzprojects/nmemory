using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using System.Linq.Expressions;
using NMemory.Linq;
using NMemory.Transactions;

namespace NMemory.Modularity
{
    /// <summary>
    /// Provides essential functionality for the database engine.
    /// </summary>
    public interface ICore : IDatabaseComponent
    {
        void RegisterEntityType<T>();

        T CreateEntity<T>();

        Table<TEntity, TPrimaryKey> CreateTable<TEntity, TPrimaryKey>(
            Expression<Func<TEntity, TPrimaryKey>> primaryKey,
            IdentitySpecification<TEntity> identitySpecification,
            IEnumerable<TEntity> initialEntities) where TEntity : class;
    }
}
