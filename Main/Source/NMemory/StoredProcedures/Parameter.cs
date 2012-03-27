using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.StoredProcedures
{
    public class Parameter<T>
    {
        public static implicit operator T(Parameter<T> parameter)
        {
            return default(T);
        }
    }
}
