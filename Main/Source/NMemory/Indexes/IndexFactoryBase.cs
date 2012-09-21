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
        private IKeyInfoFactory keyInfoFactory;

        public IndexFactoryBase() : this(new DefaultKeyInfoFactory())
        {

        }

        public IndexFactoryBase(IKeyInfoFactory keyInfoFactory)
        {
            this.keyInfoFactory = keyInfoFactory;
        }

        public IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, Expression<Func<TEntity, TKey>> keySelector)
        {
            IKeyInfo<TEntity, TKey> keyInfo = this.keyInfoFactory.Create(keySelector);

            return CreateIndex(table, keyInfo);
        }

        public IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, Expression<Func<TEntity, TUniqueKey>> keySelector)
        {
            IKeyInfo<TEntity, TUniqueKey> keyInfo = this.keyInfoFactory.Create(keySelector);

            return CreateUniqueIndex(table, keyInfo);
        }

        public abstract IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, IKeyInfo<TEntity, TKey> keyInfo);

        public abstract IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, IKeyInfo<TEntity, TUniqueKey> keyInfo);
    }
}
