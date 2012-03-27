using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NMemory.Tables;

namespace NMemory.Indexes
{
    public interface IIndex
    {
        ITable Table { get; }

        IKeyInfo KeyInfo { get; }

        bool SupportsIntervalSearch { get; }

        long Count { get; }
    }
}
