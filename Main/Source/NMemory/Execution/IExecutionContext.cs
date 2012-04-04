using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;

namespace NMemory.Execution
{
    public interface IExecutionContext
    {
        T GetParameter<T>(string name);

        IList<ITable> Tables { get; }
    }
}
