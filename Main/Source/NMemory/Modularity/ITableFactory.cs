using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NMemory.Tables;
using NMemory.Indexes;

namespace NMemory.Modularity
{
    /// <summary>
    /// Provides essential functionality for the database engine.
    /// </summary>
    public interface ITableFactory : IDatabaseComponent
    {
        Table<TEntity, TPrimaryKey> CreateTable<TEntity, TPrimaryKey>(
            IKeyInfo<TEntity, TPrimaryKey> primaryKey,
            IdentitySpecification<TEntity> identitySpecification)

            where TEntity : class;
    }
}
