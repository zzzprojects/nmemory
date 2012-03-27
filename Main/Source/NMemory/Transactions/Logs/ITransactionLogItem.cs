using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Transactions.Logs
{
    internal interface ITransactionLogItem
    {
        void Undo();
    }
}
