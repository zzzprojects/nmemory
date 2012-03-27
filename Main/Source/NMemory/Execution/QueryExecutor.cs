using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using NMemory.Diagnostics;
using NMemory.Diagnostics.Messages;
using NMemory.Linq;
using NMemory.Modularity;
using NMemory.Concurrency;

namespace NMemory.Execution
{
    public class QueryExecutor : QueryExecutorBase
    {

        public bool EnableCompilationCaching { get; set; }

        public bool EnableOptimization { get; set; }

        public override IQueryEnumeratorFactory CreateQueryEnumeratorFactory()
        {
            return new QueryEnumeratorFactory();
        }
    }
}
