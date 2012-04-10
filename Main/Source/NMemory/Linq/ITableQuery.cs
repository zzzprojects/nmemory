using System;
using NMemory.Modularity;

namespace NMemory.Linq
{
    public interface ITableQuery
    {
        Type EntityType { get; }

        IDatabase Database { get; }
    }
}
