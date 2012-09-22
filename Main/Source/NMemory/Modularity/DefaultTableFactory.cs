using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NMemory.Execution;
using NMemory.Tables;
using NMemory.Indexes;

namespace NMemory.Modularity
{
    internal class DefaultTableFactory : ITableFactory
    {
        private IDatabase database;

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }

        public Table<TEntity, TPrimaryKey> CreateTable<TEntity, TPrimaryKey>(
            IKeyInfo<TEntity, TPrimaryKey> primaryKey,
            IdentitySpecification<TEntity> identitySpecification)

            where TEntity : class
        {
            Table<TEntity, TPrimaryKey> table = new DefaultTable<TEntity, TPrimaryKey>(database, primaryKey, identitySpecification);
            this.RegisterTable(table);
            
            return table;
        }

        private void RegisterTable(ITable table)
        {
            TableLockConcurrencyManager manager = this.database.DatabaseEngine.ConcurrencyManager as TableLockConcurrencyManager;
            manager.RegisterTable(table);
        }
    }
}
