using NMemory.StoredProcedures;
using NMemory.Tables;

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
