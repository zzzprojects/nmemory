using System;
using NMemory.Tables;
using NMemory.Concurrency;
using System.Collections.Generic;

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

        public void OnTableCreated(ITable table)
        {
            ConcurrencyManager manager = this.database.ConcurrencyManager as ConcurrencyManager;

            manager.RegisterTable(table);
        }

        public T CreateEntity<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }
}
