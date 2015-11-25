// -----------------------------------------------------------------------------------
// <copyright file="Transaction.cs" company="NMemory Team">
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
// -----------------------------------------------------------------------------------

namespace NMemory.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NMemory.Concurrency.Locks;
    using NMemory.Modularity;

    public class Transaction : System.Transactions.IEnlistmentNotification
    {
        #region Static members

        private static readonly TimeSpan DefaultTransactionTimeout = TimeSpan.FromSeconds(
#if DEBUG 
        60 * 10
#else
        30
#endif
        );

        private static int transactionCounter;

        #endregion

        #region Static helpers

        public static Transaction Create(System.Transactions.Transaction external)
        {
            return new Transaction(external, false);
        }

        public static TransactionContext EnsureTransaction(ref Transaction transaction, IDatabase database)
        {
            TransactionContext result = null;

            if (transaction != null)
            {
                if (transaction.Aborted)
                {
                    throw new System.Transactions.TransactionAbortedException();
                }

                // Just a mock result
                result = new TransactionContext();
            }
            else
            {
                // Create a local transaction for the current operation

                System.Transactions.CommittableTransaction localTransaction = CreateDefaultTransaction();
                transaction = Create(localTransaction);

                result = new TransactionContext(localTransaction);
            }

            transaction.Subscribe(database.DatabaseEngine.TransactionHandler);
            // database.DatabaseEngine.TransactionHandler.EnsureSubscription(transaction);

            return result;
        }

        public static Transaction TryGetAmbientEnlistedTransaction()
        {
            System.Transactions.Transaction ambientTransaction = System.Transactions.Transaction.Current;

            if (ambientTransaction == null)
            {
                return null;
            }

            return AmbientTransactionStore.GetAmbientEnlistedTransaction(ambientTransaction);
        }

        private static System.Transactions.CommittableTransaction CreateDefaultTransaction()
        {
            System.Transactions.CommittableTransaction transaction = new System.Transactions.CommittableTransaction(
                new System.Transactions.TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = DefaultTransactionTimeout
                });

            return transaction;
        }

        #endregion

        private int transactionId;
        private bool isAmbient;
        private System.Transactions.Transaction internalTransaction;
        private IsolationLevels isolationLevel;
        private LightweightSpinLock atomicSectionLock;

        private volatile bool aborted;

        private HashSet<ITransactionHandler> registeredHandlers;

        public Transaction(System.Transactions.Transaction transaction, bool isAmbient)
        {
            this.internalTransaction = transaction;
            this.isAmbient = isAmbient;

            this.transactionId = Interlocked.Increment(ref transactionCounter);
            this.atomicSectionLock = new LightweightSpinLock();
            this.isolationLevel = this.MapIsolationLevel(transaction.IsolationLevel);

            this.aborted = false;

            this.registeredHandlers = new HashSet<ITransactionHandler>();

            this.internalTransaction.EnlistVolatile(this, System.Transactions.EnlistmentOptions.EnlistDuringPrepareRequired);
        }


        internal int Id
        {
            get { return this.transactionId; }
        }

        public IsolationLevels IsolationLevel
        {
            get { return this.isolationLevel; }
        }

        internal System.Transactions.Transaction InternalTransaction
        {
            get { return this.internalTransaction; }
        }

        internal bool Aborted
        {
            get { return this.aborted; }
        }

        private IsolationLevels MapIsolationLevel(System.Transactions.IsolationLevel isolationLevel)
        {
            switch (isolationLevel)
            {
                case System.Transactions.IsolationLevel.Chaos:
                case System.Transactions.IsolationLevel.ReadUncommitted:
                case System.Transactions.IsolationLevel.Unspecified:
                case System.Transactions.IsolationLevel.ReadCommitted:
                    return IsolationLevels.ReadCommited;
                case System.Transactions.IsolationLevel.RepeatableRead:
                case System.Transactions.IsolationLevel.Serializable:
                case System.Transactions.IsolationLevel.Snapshot:
                    return IsolationLevels.RepetableRead;
                default:
                    throw new NotSupportedException();
            }
        }

        internal void Subscribe(ITransactionHandler handler)
        {
            if (!this.registeredHandlers.Contains(handler))
            {
                this.registeredHandlers.Add(handler);
            }
        }

        public void EnterAtomicSection()
        {
            this.atomicSectionLock.Enter();

            if (this.aborted)
            {
                this.atomicSectionLock.Exit();
                throw new System.Transactions.TransactionAbortedException();
            }
        }

		public void ExitAtomicSection()
        {
            this.atomicSectionLock.Exit();
        }

        private void Commit()
        {
            foreach (ITransactionHandler handler in this.registeredHandlers)
            {
                handler.Commit(this);
            }
        }

        private void Rollback()
        {
            foreach (ITransactionHandler handler in this.registeredHandlers)
            {
                handler.Rollback(this);
            }
        }

        private void Release()
        {
            
        }

        #region Transaction enlistment

        void System.Transactions.IEnlistmentNotification.Commit(System.Transactions.Enlistment enlistment)
        {
            this.Commit();
            this.Release();

            enlistment.Done();
        }

        void System.Transactions.IEnlistmentNotification.Rollback(System.Transactions.Enlistment enlistment)
        {
            ////bool timeout = Transaction.current != this;

            // Rollback can be concurrent (timeout)
            this.EnterAtomicSection();

            this.aborted = true;
            this.Rollback();
            this.Release();

            this.ExitAtomicSection();

            enlistment.Done();
        }

        void System.Transactions.IEnlistmentNotification.InDoubt(System.Transactions.Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        void System.Transactions.IEnlistmentNotification.Prepare(System.Transactions.PreparingEnlistment preparingEnlistment)
        {
            if (this.aborted)
            {
                preparingEnlistment.ForceRollback();

                this.Rollback();
                this.Release();

                preparingEnlistment.Done();
            }
            else
            {
                preparingEnlistment.Prepared();
            }
        }


        #endregion
        
    }
}
