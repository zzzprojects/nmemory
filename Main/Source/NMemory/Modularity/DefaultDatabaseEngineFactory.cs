using NMemory.Execution;

namespace NMemory.Modularity
{
    public class DefaultDatabaseEngineFactory : IDatabaseEngineFactory
    {
        public IConcurrencyManager CreateConcurrencyManager()
        {
            return new TableLockConcurrencyManager();
        }

        public ICore CreateCore()
        {
            return new DefaultCore();
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
