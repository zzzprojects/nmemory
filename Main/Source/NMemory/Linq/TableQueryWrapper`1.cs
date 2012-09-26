using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using NMemory.Transactions;

namespace NMemory.Linq
{
    internal class TableQueryWrapper<T> : IEnumerable<T>
    {
        private TableQuery<T> tableQuery;
        private Transaction transaction;
        private IDictionary<string, object> parameters;


        public TableQueryWrapper(TableQuery<T> tableQuery, IDictionary<string, object> parameters, Transaction transaction)
        {
            this.tableQuery = tableQuery;
            this.transaction = transaction;
            this.parameters = parameters;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.tableQuery.GetEnumerator(this.parameters, this.transaction);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
