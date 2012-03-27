using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;

namespace NMemory.Linq
{
    internal interface IIndexedQueryable<T> : IQueryable<T>
    {
        IIndex Index { get; }
    }
}
