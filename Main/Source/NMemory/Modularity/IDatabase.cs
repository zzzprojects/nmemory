using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.StoredProcedures;

namespace NMemory.Modularity
{
    public interface IDatabase
    {
        TableCollection Tables
        {
            get;
        }

        StoredProcedureCollection StoredProcedures
        {
            get;
        }

        IDatabaseEngine DatabaseEngine
        {
            get;
        }
    }
}
