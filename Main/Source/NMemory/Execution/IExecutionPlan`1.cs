
namespace NMemory.Execution
{
    public interface IExecutionPlan<T> : IExecutionPlan
    {
        T Execute(IExecutionContext context);
    }
}
