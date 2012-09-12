using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NMemory.Transactions.Logs;
using System.Collections.Concurrent;
using NMemory.Modularity;

namespace NMemory.Transactions
{
    public class TransactionHandler : ITransactionHandler
    {
        private IDatabase database;
        private ConcurrentDictionary<Transaction, TransactionLog> transactionLogs;

        private static int counter;
        private int id;

        public TransactionHandler()
        {
            this.id = Interlocked.Increment(ref counter);
            this.transactionLogs = new ConcurrentDictionary<Transaction, TransactionLog>();
        }

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }


        public ITransactionLog GetTransactionLog(Transaction transaction)
        {
            return this.transactionLogs.GetOrAdd(transaction, tran => new TransactionLog());
        }

        public void Commit(Transaction transaction)
        {
            this.ReleaseResources(transaction);
        }

        public void Rollback(Transaction transaction)
        {
            GetTransactionLog(transaction).Rollback();

            this.ReleaseResources(transaction);
        }


        private void ReleaseResources(Transaction transaction)
        {
            TransactionLog log;
            this.transactionLogs.TryRemove(transaction, out log);

            if (log != null)
            {
                log.Release();
            }

            this.database.DatabaseEngine.ConcurrencyManager.ReleaseAllLocks(transaction);
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
