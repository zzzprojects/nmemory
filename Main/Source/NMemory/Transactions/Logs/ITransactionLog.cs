using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Transactions.Logs
{
    public interface ITransactionLog
    {
        int CurrentPosition { get; }

        void Rollback();

        void RollbackTo(int position);

        void Write(ITransactionLogItem item);
    }
}
