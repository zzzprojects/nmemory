// ----------------------------------------------------------------------------------
// <copyright file="TableLockConcurrencyManager.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.Concurrency
{
    using System;
    using System.Collections.Generic;
    using NMemory.Concurrency.Locks;
    using NMemory.DataStructures.Internal.Graphs;
    using NMemory.Exceptions;
    using NMemory.Modularity;
    using NMemory.Tables;
    using NMemory.Transactions;

    public class TableLockConcurrencyManager : IConcurrencyManager, ITableCatalog
    {
        private IDatabase database;

        // private DeadlockManagementStrategies deadlockManagement;
        private ILockFactory lockFactory;

        private Dictionary<ITable, ILock> tableLocks;
        private Graph<object> lockGraph;
        private TransactionLockInventory lockInventory;

        public TableLockConcurrencyManager()
        {
            // this.deadlockManagement = DeadlockManagementStrategies.DeadlockDetection;
            this.lockFactory = new DefaultLockFactory();

            this.tableLocks = new Dictionary<ITable, ILock>();
            this.lockGraph = new Graph<object>();
            this.lockInventory = new TransactionLockInventory();
        }

        public void Initialize(IDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            this.database = database;
        }

        public void RegisterTable(ITable table)
        {
            this.tableLocks.Add(table, this.lockFactory.CreateLock());
        }

        public void AcquireTableWriteLock(ITable table, Transaction transaction)
        {
            IList<ITable> tables = this.database.Tables.GetAllTables();
            ILock tableLock = this.tableLocks[table];

            var lockInfo = this.lockInventory.GetLockInformation(tableLock, transaction);

            if (!lockInfo.IsWriteLockHeld)
            {
                // TODO: OnWaiting
                if (lockInfo.IsReadLockHeld)
                {
                    tableLock.Upgrade();
                }
                else
                {
                    tableLock.EnterWrite();
                }

                // TODO: OnReleased
                lockInfo.IsWriteLockHeld = true;
            }

            ////switch (this.deadlockManagement)
            ////{
            ////    #region Deadlock detection

            ////    case DeadlockManagementStrategies.DeadlockDetection:

            ////        if (!tableLock.IsWriteLockHeld)
            ////        {
            ////            this.WaitsFor(tableLock);

            ////            tableLock.EnterWriteLock();

            ////            this.LockAquired(tableLock);
            ////        }
            ////        break;
            ////    #endregion

            ////    #region Deadlock prevention

            ////    case DeadlockManagementStrategies.DeadlockPrevention:

            ////        throw new NotSupportedException();

            ////        if (!tableLock.IsWriteLockHeld)
            ////        {
            ////            for (int i = 0; i < tables.Count; i++)
            ////            {
            ////                if (tables[i] == table)
            ////                {
            ////                    for (int l = i + 1; l < tables.Count; l++)
            ////                    {
            ////                        ILock otherLock = this.tableLocks[tables[l]];

            ////                        if (!otherLock.IsWriteLockHeld)
            ////                        {
            ////                            this.WaitsFor(otherLock);
            ////                            otherLock.EnterWriteLock();
            ////                            this.LockAquired(otherLock);
            ////                        }
            ////                    }

            ////                    this.WaitsFor(tableLock);
            ////                    tableLock.EnterWriteLock();
            ////                    this.LockAquired(tableLock);
            ////                    break;
            ////                }
            ////            }
            ////        }
                    
            ////        break;
                        
            ////    #endregion
            ////}
        }

        public void ReleaseTableWriteLock(ITable table, Transaction transaction)
        {
            if (transaction.IsolationLevel == IsolationLevels.ReadCommited ||
                transaction.IsolationLevel == IsolationLevels.RepetableRead)
            {
                return;
            }

            ILock tableLock = this.tableLocks[table];
            var lockInfo = this.lockInventory.GetLockInformation(tableLock, transaction);

            if (lockInfo.IsWriteLockHeld)
            {
                if (lockInfo.IsReadLockHeld)
                {
                    tableLock.Downgrade();
                }
                else
                {
                    tableLock.ExitWrite();
                    // TODO: OnReleased
                }

                lockInfo.IsWriteLockHeld = false;
            }
        }

        public void AcquireRelatedTableLock(ITable table, Transaction transaction)
        {
            ILock tableLock = this.tableLocks[table];
            var lockInfo = this.lockInventory.GetLockInformation(tableLock, transaction);

            if (!lockInfo.IsReadLockHeld)
            {
                if (!lockInfo.IsWriteLockHeld)
                {
                    // TODO: OnWaiting
                    tableLock.EnterRead();
                    // TODO: OnAcquired
                }

                lockInfo.IsReadLockHeld = true;
            }

            lockInfo.IsRelatedTable = true;
        }

        public void AcquireTableReadLock(ITable table, Transaction transaction)
        {
            ILock tableLock = this.tableLocks[table];
            var lockInfo = this.lockInventory.GetLockInformation(tableLock, transaction);

            if (!lockInfo.IsReadLockHeld)
            {
                if (!lockInfo.IsWriteLockHeld)
                {
                    // TODO: OnWaiting
                    tableLock.EnterRead();
                    // TODO: OnAcquired
                }

                lockInfo.IsReadLockHeld = true;
            }
        }

        public void ReleaseTableReadLock(ITable table, Transaction transaction)
        {
            if (transaction.IsolationLevel == IsolationLevels.RepetableRead)
            {
                return;
            }

            ILock tableLock = this.tableLocks[table];
            var lockInfo = this.lockInventory.GetLockInformation(tableLock, transaction);

            // Releated tables are locked until the transaction ends
            if (!lockInfo.IsRelatedTable && lockInfo.IsReadLockHeld)
            {
                if (!lockInfo.IsWriteLockHeld)
                {
                    tableLock.ExitRead();
                    // TODO: OnReleased
                }

                lockInfo.IsReadLockHeld = false;
            }
        }

        public void ReleaseAllLocks(Transaction transaction)
        {
            foreach (var item in this.lockInventory.GetAllForRelease(transaction))
            {
                if (item.IsWriteLockHeld)
                {
                    item.Lock.ExitWrite();
                }
                else if (item.IsReadLockHeld)
                {
                    item.Lock.ExitRead();
                }

                // TODO: Graph
                ////lockGraph.RemoveConnection(item.Lock, transaction);
            }
        }

        private void WaitsFor(ILock l, Transaction transaction)
        {
            this.lockGraph.AddConnection(transaction, l);

            if (this.lockGraph.HasCycle())
            {
                // Deadlock detection
                throw new DeadlockException();
            }
        }

        private void LockAquired(ILock l, Transaction transaction)
        {
            this.lockGraph.RemoveConnection(transaction, l);
            this.lockGraph.AddConnection(l, transaction);
        }
    }
}
