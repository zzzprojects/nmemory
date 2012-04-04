using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Linq;
using NMemory.Transactions;
using System.Linq.Expressions;
using NMemory.Execution;

namespace NMemory.Modularity
{
    public interface IQueryExecutor : IDatabaseComponent
    {
        IEnumerator<T> Execute<T>(Func<IExecutionContext, IEnumerable<T>> compiledQuery, IExecutionContext context);

        T Execute<T>(Func<IExecutionContext, T> compiledQuery, IExecutionContext context);
    }
}
