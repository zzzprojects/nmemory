using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace NMemory.Transactions
{
    internal static class AmbientTransactionStore
    {
        private static ConcurrentDictionary<string, Transaction> transactions;

        static AmbientTransactionStore()
        {
            transactions = new ConcurrentDictionary<string, Transaction>();
        }

        public static Transaction GetAmbientEnlistedTransaction(System.Transactions.Transaction ambient)
        {
            return transactions.GetOrAdd(ambient.TransactionInformation.LocalIdentifier, x => CreateTransaction(ambient));
        }

        public static void RemoveTransaction(System.Transactions.Transaction ambient)
        {
            Transaction removedTransaction;
            transactions.TryRemove(ambient.TransactionInformation.LocalIdentifier, out removedTransaction);
        }


        private static Transaction CreateTransaction(System.Transactions.Transaction ambient)
        {
            ambient.TransactionCompleted += OnAmbientTransactionCompleted;

            return new Transaction(ambient, true);
        }

        private static void OnAmbientTransactionCompleted(object sender, System.Transactions.TransactionEventArgs e)
        {
            e.Transaction.TransactionCompleted -= OnAmbientTransactionCompleted;
            RemoveTransaction(e.Transaction);
        }
    }
}
