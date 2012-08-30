using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.DataStructures;
using NMemory.Tables;
using System.Collections;
using System.Diagnostics;

namespace NMemory.Indexes
{
    public interface IIndexFactory<TEntity>
        where TEntity : class
    {
        IIndex<TEntity, TKey> CreateIndex<TKey>(
            ITable<TEntity> table, 
            Expression<Func<TEntity, TKey>> keySelector);

        IIndex<TEntity, TKey> CreateIndex<TKey>(
            ITable<TEntity> table, 
            IKeyInfo<TEntity, TKey> keyInfo);

        IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
            ITable<TEntity> table,
            Expression<Func<TEntity, TUniqueKey>> keySelector);

        IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
            ITable<TEntity> table,
            IKeyInfo<TEntity, TUniqueKey> keyInfo);
    }

}

