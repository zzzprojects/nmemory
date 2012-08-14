using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using NMemory.Transactions;

namespace NMemory.Linq
{
    internal class TableQueryTransactionWrapper<T> : IEnumerable<T>
    {
        private TableQuery<T> tableQuery;
        private Transaction transaction;

        public TableQueryTransactionWrapper(TableQuery<T> tableQuery, Transaction transaction)
        {
            this.tableQuery = tableQuery;
            this.transaction = transaction;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.tableQuery.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
