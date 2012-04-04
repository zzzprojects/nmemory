using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Linq;
using System.Linq.Expressions;

namespace NMemory.Tables
{
    internal interface IBatchTable<TEntity>
    {
        int Update(TableQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updater);

        int Delete(TableQuery<TEntity> query);
    }
}
