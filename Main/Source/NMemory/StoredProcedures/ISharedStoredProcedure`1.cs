using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Modularity;
using NMemory.Transactions;
using System.Collections;

namespace NMemory.StoredProcedures
{
    public interface ISharedStoredProcedure<TEntity> : ISharedStoredProcedure
    {
        new IEnumerable<TEntity> Execute(IDatabase database, IDictionary<string, object> parameters);

        new IEnumerable<TEntity> Execute(IDatabase database, IDictionary<string, object> parameters, Transaction transaction);
    }
}
