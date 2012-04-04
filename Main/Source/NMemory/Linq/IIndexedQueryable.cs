using System.Linq;
using NMemory.Indexes;

namespace NMemory.Linq
{
    internal interface IIndexedQueryable<T> : IQueryable<T>
    {
        IIndex Index { get; }
    }
}
