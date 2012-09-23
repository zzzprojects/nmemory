using System.Collections.Generic;
using NMemory.Execution;

namespace NMemory.Modularity
{
    public interface IQueryExecutor : IDatabaseComponent
    {
        IEnumerator<T> Execute<T>(IExecutionPlan<IEnumerable<T>> plan, IExecutionContext context);

        T Execute<T>(IExecutionPlan<T> plan, IExecutionContext context);
    }
}
