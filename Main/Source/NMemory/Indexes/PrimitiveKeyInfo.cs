using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Indexes
{
    public static class PrimitiveKeyInfo
    {
        public static PrimitiveKeyInfo<TEntity, TKey> Create<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector)
            where TEntity : class
        {
            return new PrimitiveKeyInfo<TEntity, TKey>(keySelector);
        }

        public static PrimitiveKeyInfo<TEntity, TKey> Create<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, SortOrder sortOrder)
            where TEntity : class
        {
            return new PrimitiveKeyInfo<TEntity, TKey>(keySelector, sortOrder);
        }
    }
}
