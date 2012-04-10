using System;
using System.Collections.Generic;
using NMemory.Execution;

namespace NMemory.Modularity
{
    public interface IQueryExecutor : IDatabaseComponent
    {
        IEnumerator<T> Execute<T>(Func<IExecutionContext, IEnumerable<T>> compiledQuery, IExecutionContext context);

        T Execute<T>(Func<IExecutionContext, T> compiledQuery, IExecutionContext context);
    }
}
