using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Indexes
{
    public interface IUniqueIndex<TEntity, TUniqueKey> : 
        IIndex<TEntity, TUniqueKey>,
        IUniqueIndex<TEntity>

        where TEntity : class
    {
        TEntity GetByUniqueIndex(TUniqueKey entity);
    }
}
