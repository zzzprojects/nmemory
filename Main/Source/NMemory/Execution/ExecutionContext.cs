using System.Collections.Generic;
using NMemory.Exceptions;
using NMemory.Modularity;
using NMemory.Transactions;

namespace NMemory.Execution
{
    internal class ExecutionContext : IExecutionContext
    {
        private Transaction transaction;
        private IDictionary<string, object> parameters;
        private IDatabase database;
       
        public ExecutionContext(IDatabase database, Transaction transaction)
        {
            this.database = database;
            this.transaction = transaction;
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(IDatabase database, Transaction transaction,  IDictionary<string, object> parameters)
        {
            this.database = database;
            this.transaction = transaction;
            this.parameters = parameters;

            // Ensure initialization
            if (this.parameters == null)
            {
                this.parameters = new Dictionary<string, object>();
            }
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

        public Transaction Transaction
        {
            get { return this.transaction; }
        }
    }
}
