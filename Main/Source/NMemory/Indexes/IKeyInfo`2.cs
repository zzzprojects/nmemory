using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Indexes
{
    public interface IKeyInfo<TEntity, TKey> : IKeyInfo<TKey>, IKeyInfo
        where TEntity : class
    {
        TKey SelectKey(TEntity entity);
    }
}
