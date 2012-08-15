using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
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
