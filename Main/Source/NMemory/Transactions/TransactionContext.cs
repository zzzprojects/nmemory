using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Transactions
{
    public class TransactionContext : IDisposable
    {
        private System.Transactions.CommittableTransaction transaction;
        private bool completed;

        public TransactionContext()
	    {
            this.transaction = null;
            this.completed = false;
	    }

        public TransactionContext(System.Transactions.CommittableTransaction transaction)
        {
            this.transaction = transaction;
            this.completed = false;
        }

        public void Complete()
        {
            this.completed = true;
        }

        public void Dispose()
        {
            if (this.transaction != null)
            {
                if (this.completed)
                {
                    this.transaction.Commit();
                }
                else
                {
                    this.transaction.Rollback();
                }
            }
        }
    }
}
