using System;
using System.Linq.Expressions;
using NMemory.Execution;

namespace NMemory.Modularity
{
    public interface IQueryCompiler : IDatabaseComponent
    {
        IExecutionPlan<T> Compile<T>(Expression expression);
    }
}
