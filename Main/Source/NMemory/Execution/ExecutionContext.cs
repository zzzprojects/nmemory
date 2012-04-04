using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Exceptions;

namespace NMemory.Execution
{
    internal class ExecutionContext : IExecutionContext
    {
        private IList<ITable> tables;
        private IDictionary<string, object> parameters;

        public ExecutionContext()
        {
            this.tables = new List<ITable>();
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(IList<ITable> tables)
        {
            this.tables = tables;
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(IList<ITable> tables, IDictionary<string, object> parameters)
        {
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

        public IList<ITable> Tables
        {
            get { return this.tables; }
        }


    }
}
