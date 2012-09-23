using System.Linq.Expressions;

namespace NMemory.Execution
{
    public interface IExecutionPlan
    {
        Expression Plan { get; }
    }
}
