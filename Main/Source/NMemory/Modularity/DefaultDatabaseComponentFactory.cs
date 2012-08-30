using NMemory.Execution;
using NMemory.Diagnostics;

namespace NMemory.Modularity
{
    public class DefaultDatabaseComponentFactory : IDatabaseComponentFactory
    {
        public virtual IConcurrencyManager CreateConcurrencyManager()
        {
            return new TableLockConcurrencyManager();
        }

        public virtual ITableFactory CreateTableFactory()
        {
            return new DefaultTableFactory();
        }

        public virtual IQueryCompiler CreateQueryCompiler()
        {
            return new QueryCompiler() { EnableCompilationCaching = false, EnableOptimization = false };
        }

        public virtual IQueryExecutor CreateQueryExecutor()
        {
            return new QueryExecutor();
        }

        public virtual ILoggingPort CreateLoggingPort()
        {
            return new ConsoleLoggingPort();
        }
    }
}
