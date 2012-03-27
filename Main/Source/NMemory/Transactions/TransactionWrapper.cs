using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace NMemory.Transactions
{
    public class TransactionWrapper : IDisposable
    {
        private TransactionScope scope;

        public TransactionWrapper(TransactionScope scope)
        {
            this.scope = scope;
        }

        public void Complete()
        {
            if (scope != null)
            {
                scope.Complete();
            }
        }

        public void Dispose()
        {
            if (scope != null)
            {
                scope.Dispose();
            }
        }
    }
}
