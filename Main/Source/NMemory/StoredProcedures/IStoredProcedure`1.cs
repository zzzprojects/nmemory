using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.StoredProcedures
{
    public interface IStoredProcedure<T> : IStoredProcedure
    {
        new IEnumerable<T> Execute(IDictionary<string, object> parameters);
    }
}
