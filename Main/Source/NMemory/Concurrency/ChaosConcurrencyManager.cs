using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Modularity;
using NMemory.Tables;
using NMemory.Transactions;

namespace NMemory.Execution
{
    public class ChaosConcurrencyManager : IConcurrencyManager
    {
        public void Initialize(IDatabase database)
        {
            
        }

        public void AcquireTableWriteLock(ITable table, Transaction transaction)
        {
            
        }

        public void ReleaseTableWriteLock(ITable table, Transaction transaction)
        {
            
        }

        public void AcquireTableReadLock(ITable table, Transaction transaction)
        {
            
        }

        public void ReleaseTableReadLock(ITable table, Transaction transaction)
        {
            
        }

        public void ReleaseAllLocks(Transaction transaction)
        {
            
        }

        public void AcquireRelatedTableLock(ITable table, Transaction transaction)
        {
            
        }
    }
}
