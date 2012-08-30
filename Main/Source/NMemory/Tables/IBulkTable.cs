using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Linq;
using System.Linq.Expressions;
using NMemory.Transactions;

namespace NMemory.Tables
{
    internal interface IBulkTable<TEntity>
        where TEntity : class
    {
        IEnumerable<TEntity> Update(TableQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updater, Transaction transaction);

        int Delete(TableQuery<TEntity> query, Transaction transaction);
    }
}
