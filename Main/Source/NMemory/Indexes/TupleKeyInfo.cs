using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Indexes
{
    public static class TupleKeyInfo
    {
        public static TupleKeyInfo<TEntity, TKey> Create<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector)
            where TEntity : class
        {
            return new TupleKeyInfo<TEntity, TKey>(keySelector);
        }
    }
}
