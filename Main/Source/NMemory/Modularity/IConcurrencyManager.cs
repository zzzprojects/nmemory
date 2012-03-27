using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Transactions;

namespace NMemory.Modularity
{
    public interface IConcurrencyManager : IDatabaseComponent
    {
        void AcquireTableWriteLock(ITable table, Transaction transaction);

        void ReleaseTableWriteLock(ITable table, Transaction transaction);


        void AcquireTableReadLock(ITable table, Transaction transaction);

        void ReleaseTableReadLock(ITable table, Transaction transaction);


        void ReleaseAllLocks(Transaction transaction);
    }
}
