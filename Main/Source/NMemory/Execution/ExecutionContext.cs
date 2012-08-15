using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Exceptions;
using NMemory.Transactions;

namespace NMemory.Execution
{
    internal class ExecutionContext : IExecutionContext
    {
        private Transaction transaction;
        private IList<ITable> tables;
        private IDictionary<string, object> parameters;
       
        public ExecutionContext(Transaction transaction)
        {
            this.transaction = transaction;
            this.tables = new List<ITable>();
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(Transaction transaction, IList<ITable> tables)
        {
            this.transaction = transaction;
            this.tables = tables;
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(Transaction transaction, IList<ITable> tables, IDictionary<string, object> parameters)
        {
            this.transaction = transaction;
            this.tables = tables;
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

        public IList<ITable> AffectedTables
        {
            get { return this.tables; }
        }

        public Transaction Transaction
        {
            get { return this.transaction; }
        }
    }
}
