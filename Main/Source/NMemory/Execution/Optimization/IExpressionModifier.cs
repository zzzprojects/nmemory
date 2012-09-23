using System.Linq.Expressions;

namespace NMemory.Execution.Optimization
{
    public interface IExpressionModifier
    {
        Expression ModifyExpression(Expression expression);
    }
}
