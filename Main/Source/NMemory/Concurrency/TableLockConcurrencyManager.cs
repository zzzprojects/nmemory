using System;
using System.Collections.Generic;
using NMemory.Execution.Locks;
using NMemory.DataStructures.Internal.Graphs;
using NMemory.Exceptions;
using NMemory.Modularity;
using NMemory.Tables;
using NMemory.Transactions;

namespace NMemory.Execution
{
    public class TableLockConcurrencyManager : IConcurrencyManager
    {
        private Database database;

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

        public void Initialize(Database database)
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

            #region Old

            //switch (this.deadlockManagement)
            //{
            //    #region Deadlock detection

            //    case DeadlockManagementStrategies.DeadlockDetection:

            //        if (!tableLock.IsWriteLockHeld)
            //        {
            //            this.WaitsFor(tableLock);

            //            tableLock.EnterWriteLock();

            //            this.LockAquired(tableLock);
            //        }
            //        break;
            //    #endregion

            //    #region Deadlock prevention

            //    case DeadlockManagementStrategies.DeadlockPrevention:

            //        throw new NotSupportedException();

            //        if (!tableLock.IsWriteLockHeld)
            //        {
            //            for (int i = 0; i < tables.Count; i++)
            //            {
            //                if (tables[i] == table)
            //                {
            //                    for (int l = i + 1; l < tables.Count; l++)
            //                    {
            //                        ILock otherLock = this.tableLocks[tables[l]];

            //                        if (!otherLock.IsWriteLockHeld)
            //                        {
            //                            this.WaitsFor(otherLock);
            //                            otherLock.EnterWriteLock();
            //                            this.LockAquired(otherLock);
            //                        }
            //                    }

            //                    this.WaitsFor(tableLock);
            //                    tableLock.EnterWriteLock();
            //                    this.LockAquired(tableLock);
            //                    break;
            //                }
            //            }
            //        }
                    
            //        break;
                        
            //    #endregion
            //}

            #endregion
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
            lockGraph.AddConnection(transaction, l);

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
