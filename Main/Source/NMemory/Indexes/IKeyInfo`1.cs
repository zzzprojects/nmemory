using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Indexes
{
    public interface IKeyInfo<TKey> : IKeyInfo
    {
        bool IsEmptyKey(TKey key);

        IComparer<TKey> KeyComparer { get; }
    }
}
