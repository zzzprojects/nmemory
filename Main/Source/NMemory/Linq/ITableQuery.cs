using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Linq
{
    public interface ITableQuery
    {
        Type EntityType { get; }

        Database Database { get; }
    }
}
