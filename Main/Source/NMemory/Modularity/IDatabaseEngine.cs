using NMemory.Tables;
using NMemory.Transactions;

namespace NMemory.Modularity
{
    public interface IDatabaseEngine
    {
        ITableFactory TableFactory
        {
            get;
        }

        IConcurrencyManager ConcurrencyManager
        {
            get;
        }

        IQueryCompiler Compiler
        {
            get;
        }

        IQueryExecutor Executor
        {
            get;
        }

        ITransactionHandler TransactionHandler
        {
            get;
        }

        ILoggingPort LoggingPort
        {
            get;
        }
    }
}
