using NMemory.Transactions;
using NMemory.Transactions.Logs;

namespace NMemory.Modularity
{
    public interface ITransactionHandler : IDatabaseComponent
    {
        ITransactionLog GetTransactionLog(Transaction transaction);

        void Commit(Transaction transaction);

        void Rollback(Transaction transaction);
    }
}
