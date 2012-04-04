using System;
using NMemory.Tables;
using NMemory.Execution;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NMemory.Modularity
{
    internal class DefaultCore : ICore
    {
        private Database database;

        public void Initialize(Database database)
        {
            this.database = database;
        }

        public void RegisterEntityType<T>()
        {
            
        }

        public T CreateEntity<T>()
        {
            return Activator.CreateInstance<T>();
        }


        public Table<TEntity, TPrimaryKey> CreateTable<TEntity, TPrimaryKey>(
            Expression<Func<TEntity, TPrimaryKey>> primaryKey, 
            IdentitySpecification<TEntity> identitySpecification, 
            IEnumerable<TEntity> initialEntities)  where TEntity : class
        {
            Table<TEntity, TPrimaryKey> table = new DefaultTable<TEntity, TPrimaryKey>(database, primaryKey, identitySpecification, initialEntities);

            TableLockConcurrencyManager manager = this.database.ConcurrencyManager as TableLockConcurrencyManager;
            manager.RegisterTable(table);

            return table;
        }
    }
}
