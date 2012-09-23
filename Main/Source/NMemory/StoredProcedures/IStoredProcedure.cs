using System.Collections;
using System.Collections.Generic;
using NMemory.Transactions;

namespace NMemory.StoredProcedures
{
    public interface IStoredProcedure
    {
        IList<ParameterDescription> Parameters { get; }

        IEnumerable Execute(IDictionary<string, object> parameters);

        IEnumerable Execute(IDictionary<string, object> parameters, Transaction transaction);
    }
}
