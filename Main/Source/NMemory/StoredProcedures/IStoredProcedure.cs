using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NMemory.StoredProcedures
{
    public interface IStoredProcedure
    {
        IList<ParameterDescription> Parameters { get; }

        IEnumerable Execute(IDictionary<string, object> parameters);
    }
}
