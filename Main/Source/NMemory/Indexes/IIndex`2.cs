using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Indexes
{
    public interface IIndex<TEntity, TKey> : IIndex<TEntity>
    {
        bool Contains(TKey key);

        IEnumerable<TEntity> Select(TKey value);

        IEnumerable<TEntity> Select(TKey from, TKey to, bool fromOpen, bool toOpen);

        IEnumerable<TEntity> SelectGreater(TKey from, bool open);

        IEnumerable<TEntity> SelectLess(TKey to, bool open);

        new IKeyInfo<TEntity, TKey> KeyInfo { get; }
    }
}
