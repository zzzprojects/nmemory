using NMemory.Modularity;
using NMemory.Transactions;

namespace NMemory.Execution
{
    public interface IExecutionContext
    {
        T GetParameter<T>(string name);

        IDatabase Database { get; }

        Transaction Transaction { get; }
    }
}
