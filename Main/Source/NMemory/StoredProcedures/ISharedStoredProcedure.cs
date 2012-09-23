using System.Collections;
using System.Collections.Generic;
using NMemory.Modularity;
using NMemory.Transactions;

namespace NMemory.StoredProcedures
{
    public interface ISharedStoredProcedure
    {
        IList<ParameterDescription> Parameters { get; }

        IEnumerable Execute(IDatabase database, IDictionary<string, object> parameters);

        IEnumerable Execute(IDatabase database, IDictionary<string, object> parameters, Transaction transaction);
    }
}
