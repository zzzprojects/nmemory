using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using System.Linq.Expressions;
using NMemory.DataStructures;
using NMemory.Common;

namespace NMemory.Indexes
{
    public abstract class IndexFactoryBase<TEntity> : IIndexFactory<TEntity> where TEntity : class
    {
        public IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, Expression<Func<TEntity, TKey>> keySelector)
        {
            return CreateIndex(table, CreateKeyInfo<TKey>(keySelector));
        }

        public IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, Expression<Func<TEntity, TUniqueKey>> keySelector)
        {
            return CreateUniqueIndex(table, CreateKeyInfo<TUniqueKey>(keySelector));
        }

        public abstract IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, IKeyInfo<TEntity, TKey> keyInfo);

        public abstract IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, IKeyInfo<TEntity, TUniqueKey> keyInfo);

        private static IKeyInfo<TEntity, TKey> CreateKeyInfo<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            if (typeof(TKey).IsValueType || (typeof(TKey) == typeof(string)))
            {
                return new PrimitiveKeyInfo<TEntity, TKey>(keySelector);
            }
            else if (ReflectionHelper.IsAnonymousType(typeof(TKey)))
            {
                return new AnonymousTypeKeyInfo<TEntity, TKey>(keySelector);
            }
            else if (ReflectionHelper.IsTuple(typeof(TKey)))
            {
                return new TupleKeyInfo<TEntity, TKey>(keySelector);
            }
            else
            {
                throw new ArgumentException("", "keySelector");
            }
        }

    }
}
