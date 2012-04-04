using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Indexes
{
    public interface IUniqueIndex<TEntity, TUniqueKey> : IIndex<TEntity, TUniqueKey>
    {
        TEntity GetByUniqueIndex(TUniqueKey entity);
    }
}
