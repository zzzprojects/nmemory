using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Transactions;
using NMemory.Tables;
using NMemory.Linq;

namespace NMemory.Modularity
{
    public interface IQueryEnumeratorFactory
    {
        IEnumerator<T> Create<T>(TableQuery<T> tableQuery, TransactionWrapper transactionWrapper);
    }
}
