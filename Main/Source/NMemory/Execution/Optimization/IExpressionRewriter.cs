using System.Linq.Expressions;

namespace NMemory.Execution.Optimization
{
    public interface IExpressionRewriter
    {
        Expression ModifyExpression(Expression expression);
    }
}
