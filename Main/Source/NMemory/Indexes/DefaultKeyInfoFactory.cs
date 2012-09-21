using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.Common;

namespace NMemory.Indexes
{
    public class DefaultKeyInfoFactory : IKeyInfoFactory
    {
        public IKeyInfo<TEntity, TKey> Create<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector)
            where TEntity : class
        {
            if (typeof(TKey).IsValueType || (typeof(TKey) == typeof(string)))
            {
                return PrimitiveKeyInfo.Create(keySelector);
            }
            else if (ReflectionHelper.IsAnonymousType(typeof(TKey)))
            {
                return AnonymousTypeKeyInfo.Create(keySelector);
            }
            else if (ReflectionHelper.IsTuple(typeof(TKey)))
            {
                return TupleKeyInfo.Create(keySelector);
            }
            else
            {
                throw new ArgumentException("", "keySelector");
            }
        }
    }
}
