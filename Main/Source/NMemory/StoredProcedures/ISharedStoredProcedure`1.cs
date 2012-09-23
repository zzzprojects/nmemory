using System.Collections.Generic;
using NMemory.Modularity;
using NMemory.Transactions;

namespace NMemory.StoredProcedures
{
    public interface ISharedStoredProcedure<TEntity> : ISharedStoredProcedure
    {
        new IEnumerable<TEntity> Execute(IDatabase database, IDictionary<string, object> parameters);

        new IEnumerable<TEntity> Execute(IDatabase database, IDictionary<string, object> parameters, Transaction transaction);
    }
}
