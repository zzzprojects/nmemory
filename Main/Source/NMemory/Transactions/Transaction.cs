using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NMemory.Concurrency.Locks;

namespace NMemory.Transactions
{
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


        [ThreadStatic]
        private static Transaction current;

        public static Transaction Current
        {
            get 
            {
                Transaction tran = Transaction.current;

                // Check if the transaction is enlisted on an ambient transaction
                if (tran != null && tran.isAmbient)
                {
                    // Check if that ambient transaction is still present
                    if (System.Transactions.Transaction.Current != tran.internalTransaction)
                    {
                        // If not, remove the transaction
                        Transaction.current = null;
                    }
                }

                return Transaction.current; 
            }
        }

        #endregion

        #region Static helpers

        public static void EnlistOnExternal(System.Transactions.Transaction external)
        {
            CheckExistingTransaction();

            Transaction.current = new Transaction(external, false);
        }

        internal static bool TryEnlistOnTransient()
        {
            CheckExistingTransaction();

            var tran = System.Transactions.Transaction.Current;

            if (tran == null)
            {
                return false;
            }

            Transaction.current = new Transaction(tran, true);

            return true;
        }

        public static System.Transactions.TransactionScope CreateLocal()
        {
            CheckExistingTransaction();

            // Instantiate internal transaction
            var transactionScope = new System.Transactions.TransactionScope(
                System.Transactions.TransactionScopeOption.RequiresNew,
                new System.Transactions.TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = DefaultTransactionTimeout
                });

            Transaction.current = new Transaction(System.Transactions.Transaction.Current, true);

            return transactionScope;
        }

        private static void CheckExistingTransaction()
        {
            Transaction tran = Transaction.current;

            if (tran != null)
            {
                // If the current transaction is ambient, the static member might not be erased
                if (tran.isAmbient && System.Transactions.Transaction.Current != tran.internalTransaction)
                {
                    Transaction.current = null;
                    return;
                }

                throw new InvalidOperationException("The current session is already in a transaction");
            }
        }

        #endregion

        private int transactionId;
        private bool isAmbient;
        private System.Transactions.Transaction internalTransaction;
        private IsolationLevels isolationLevel;
        private LightweightSpinLock atomicSectionLock;

        private volatile bool aborted;

        private HashSet<TransactionHandler> registeredHandlers;

        internal Transaction(System.Transactions.Transaction transaction, bool isAmbient)
        {
            this.internalTransaction = transaction;
            this.isAmbient = isAmbient;

            ////this.internalTransaction.TransactionCompleted += OnTransactionCompleted;

            this.transactionId = Interlocked.Increment(ref transactionCounter);
            this.atomicSectionLock = new LightweightSpinLock();
            this.isolationLevel = this.MapIsolationLevel(transaction.IsolationLevel);

            this.aborted = false;

            this.registeredHandlers = new HashSet<TransactionHandler>();

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

        internal bool Aborted
        {
            get { return this.aborted; }
        }

        ////private void OnTransactionCompleted(object sender, System.Transactions.TransactionEventArgs e)
        ////{
        ////    this.internalTransaction.TransactionCompleted -= OnTransactionCompleted;

        ////    if (Transaction.current == this)
        ////    {
        ////        Transaction.current = null;
        ////    }
        ////}

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

        internal void Subsribe(TransactionHandler handler)
        {
            if (!this.registeredHandlers.Contains(handler))
            {
                this.registeredHandlers.Add(handler);
            }
        }

        internal void EnterAtomicSection()
        {
            this.atomicSectionLock.Enter();

            if (this.aborted)
            {
                this.atomicSectionLock.Exit();
                throw new System.Transactions.TransactionAbortedException();
            }
        }

        internal void ExitAtomicSection()
        {
            this.atomicSectionLock.Exit();
        }


        private void Commit()
        {
            foreach (TransactionHandler handler in this.registeredHandlers)
            {
                handler.Commit(this);
            }
        }

        private void Rollback()
        {
            foreach (TransactionHandler handler in this.registeredHandlers)
            {
                handler.Rollback(this);
            }
        }

        private void Release()
        {
            // Ambient transactions are removed elsewhere
            if (!this.isAmbient && Transaction.current == this)
            {
                Transaction.current = null;
            }
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
