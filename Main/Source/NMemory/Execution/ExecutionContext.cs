using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Exceptions;
using NMemory.Transactions;
using NMemory.Modularity;

namespace NMemory.Execution
{
    internal class ExecutionContext : IExecutionContext
    {
        private Transaction transaction;
        private IList<Type> affectedEntityTypes;
        private IList<ITable> affectedTables;
        private IDictionary<string, object> parameters;
        private IDatabase database;
       
        public ExecutionContext(IDatabase database, Transaction transaction)
        {
            this.database = database;
            this.transaction = transaction;
            this.affectedEntityTypes = new List<Type>();
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(IDatabase database, Transaction transaction, IList<ITable> affectedTables)
        {
            this.database = database;
            this.transaction = transaction;
            this.affectedEntityTypes = affectedTables.Select(t => t.EntityType).ToList();
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(IDatabase database, Transaction transaction, IList<Type> affectedEntityTypes)
        {
            this.database = database;
            this.transaction = transaction;
            this.affectedEntityTypes = affectedEntityTypes;
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(IDatabase database, Transaction transaction, IList<ITable> affectedTables, IDictionary<string, object> parameters)
        {
            this.database = database;
            this.transaction = transaction;
            this.affectedEntityTypes = affectedTables.Select(t => t.EntityType).ToList();
            this.parameters = parameters;
        }

        public ExecutionContext(IDatabase database, Transaction transaction, IList<Type> affectedEntityTypes, IDictionary<string, object> parameters)
        {
            this.database = database;
            this.transaction = transaction;
            this.affectedEntityTypes = affectedEntityTypes;
            this.parameters = parameters;
        }

        public T GetParameter<T>(string name)
        {
            object result = default(T);

            if (!this.parameters.TryGetValue(name, out result))
            {
                throw new ParameterException();
            }

            return (T)result;
        }

        public IDatabase Database
        {
            get { return this.database; }
        }

        public IList<ITable> AffectedTables
        {
            get 
            {
                if (this.affectedTables == null)
                {
                    this.affectedTables = new List<ITable>();

                    foreach (Type entityType in this.affectedEntityTypes)
                    {
                        ITable table = this.Database.Tables.FindTable(entityType);

                        this.affectedTables.Add(table);
                    }
                }
                return this.affectedTables; 
            }
        }

        public Transaction Transaction
        {
            get { return this.transaction; }
        }
    }
}
