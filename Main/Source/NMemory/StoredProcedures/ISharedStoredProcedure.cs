using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Modularity;
using NMemory.Transactions;
using System.Collections;

namespace NMemory.StoredProcedures
{
    public interface ISharedStoredProcedure
    {
        IList<ParameterDescription> Parameters { get; }

        IEnumerable Execute(IDatabase database, IDictionary<string, object> parameters);

        IEnumerable Execute(IDatabase database, IDictionary<string, object> parameters, Transaction transaction);
    }
}
