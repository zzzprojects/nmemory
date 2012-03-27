using System.Linq.Expressions;

namespace NMemory.Modularity
{
    public interface IQueryExecutor : IDatabaseComponent
    {
        object Compile(Expression expression);

        IQueryEnumeratorFactory CreateQueryEnumeratorFactory();
    }
}
