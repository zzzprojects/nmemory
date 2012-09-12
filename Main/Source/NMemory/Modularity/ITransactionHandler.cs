using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Transactions.Logs;
using NMemory.Transactions;

namespace NMemory.Modularity
{
    public interface ITransactionHandler : IDatabaseComponent
    {
        ITransactionLog GetTransactionLog(Transaction transaction);

        void Commit(Transaction transaction);

        void Rollback(Transaction transaction);
    }
}
