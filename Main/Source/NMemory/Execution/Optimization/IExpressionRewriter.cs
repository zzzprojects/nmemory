using System.Linq.Expressions;

namespace NMemory.Execution.Optimization
{
    public interface IExpressionRewriter
    {
        Expression Rewrite(Expression expression);
    }
}
