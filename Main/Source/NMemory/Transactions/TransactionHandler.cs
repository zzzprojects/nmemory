using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NMemory.Transactions.Logs;
using System.Collections.Concurrent;

namespace NMemory.Transactions
{
    public class TransactionHandler
    {
        private Database database;
        private ConcurrentDictionary<Transaction, TransactionLog> transactionLogs;

        private static int counter;
        private int id;

        public TransactionHandler()
        {
            this.id = Interlocked.Increment(ref counter);
            this.transactionLogs = new ConcurrentDictionary<Transaction, TransactionLog>();
        }

        internal void Initialize(Database database)
        {
            this.database = database;
        }


        internal void EnsureSubscription(Transaction transaction)
        {
            transaction.Subsribe(this);
        }

        internal TransactionLog GetTransactionLog(Transaction transaction)
        {
            return this.transactionLogs.GetOrAdd(transaction, tran => new TransactionLog());
        }

        internal void Commit(Transaction transaction)
        {
            this.ReleaseResources(transaction);
        }

        internal void Rollback(Transaction transaction)
        {
            // TODO: Rollback
            GetTransactionLog(transaction).Rollback();

            this.ReleaseResources(transaction);
        }


        private void ReleaseResources(Transaction transaction)
        {
            TransactionLog log;
            this.transactionLogs.TryRemove(transaction, out log);
            log.Release();

            this.database.ConcurrencyManager.ReleaseAllLocks(transaction);
        }


        public override int GetHashCode()
        {
            return this.id;
        }

        public override bool Equals(object obj)
        {
            TransactionHandler handler = obj as TransactionHandler;

            return handler != null && handler.id == this.id;
        }
    }
}
