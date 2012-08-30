using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Indexes
{
    public static class AnonymousTypeKeyInfo
    {
        public static AnonymousTypeKeyInfo<TEntity, TKey> Create<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector)
            where TEntity : class
        {
            return new AnonymousTypeKeyInfo<TEntity, TKey>(keySelector);
        }
    }
}
