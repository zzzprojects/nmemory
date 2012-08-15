using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Transactions;

namespace NMemory.Execution
{
    public interface IExecutionContext
    {
        T GetParameter<T>(string name);

        IList<ITable> AffectedTables { get; }

        Transaction Transaction { get; }
    }
}
