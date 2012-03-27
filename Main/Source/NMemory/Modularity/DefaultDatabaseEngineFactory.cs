using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Concurrency;
using NMemory.Execution;

namespace NMemory.Modularity
{
    public class DefaultDatabaseEngineFactory : IDatabaseEngineFactory
    {
        public IConcurrencyManager CreateConcurrencyManager()
        {
            return new ConcurrencyManager();
        }

        public ICore CreateDispatcher()
        {
            return new DefaultCore();
        }

        public IQueryExecutor CreateQueryExecutor()
        {
            return new QueryExecutor() { EnableCompilationCaching = false, EnableOptimization = false };
        }
    }
}
