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
    public interface IIndexFactory
    {
        IIndex<TEntity, TKey> CreateIndex<TEntity, TKey>(
            ITable<TEntity> table,
            Expression<Func<TEntity, TKey>> keySelector)
            where TEntity : class;

        IIndex<TEntity, TKey> CreateIndex<TEntity, TKey>(
            ITable<TEntity> table, 
            IKeyInfo<TEntity, TKey> keyInfo)
            where TEntity : class;

        IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TEntity, TUniqueKey>(
            ITable<TEntity> table,
            Expression<Func<TEntity, TUniqueKey>> keySelector)
            where TEntity : class;

        IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TEntity, TUniqueKey>(
            ITable<TEntity> table,
            IKeyInfo<TEntity, TUniqueKey> keyInfo)
            where TEntity : class;
    }

}

