// -----------------------------------------------------------------------------------
// <copyright file="TransactionHandler.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
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
    using System.Collections.Concurrent;
    using System.Threading;
    using NMemory.Modularity;
    using NMemory.Transactions.Logs;

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
