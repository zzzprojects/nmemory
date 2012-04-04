using System.Linq.Expressions;
using System;
using NMemory.Execution;

namespace NMemory.Modularity
{
    public interface IQueryCompiler : IDatabaseComponent
    {
        Func<IExecutionContext, T> Compile<T>(Expression expression);
    }
}
