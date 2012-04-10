using NMemory.Execution;

namespace NMemory.Modularity
{
    public class DefaultDatabaseEngineFactory : IDatabaseEngineFactory
    {
        public IConcurrencyManager CreateConcurrencyManager()
        {
            return new TableLockConcurrencyManager();
        }

        public ITableFactory CreateTableFactory()
        {
            return new DefaultTableFactory();
        }

        public IQueryCompiler CreateQueryCompiler()
        {
            return new QueryCompiler() { EnableCompilationCaching = false, EnableOptimization = false };
        }

        public IQueryExecutor CreateQueryExecutor()
        {
            return new QueryExecutor();
        }
    }
}
