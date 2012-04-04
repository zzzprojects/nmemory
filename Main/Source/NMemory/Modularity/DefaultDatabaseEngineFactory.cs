using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Execution;

namespace NMemory.Modularity
{
    public class DefaultDatabaseEngineFactory : IDatabaseEngineFactory
    {
        public IConcurrencyManager CreateConcurrencyManager()
        {
            return new TableLockConcurrencyManager();
        }

        public ICore CreateDispatcher()
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
