using System;

namespace NMemory.Linq
{
    public interface ITableQuery
    {
        Type EntityType { get; }

        Database Database { get; }
    }
}
