using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Transactions;
using NMemory.Modularity;

namespace NMemory.Execution
{
    public interface IExecutionContext
    {
        T GetParameter<T>(string name);

        IList<ITable> AffectedTables { get; }

        IDatabase Database { get; }

        Transaction Transaction { get; }
    }
}
