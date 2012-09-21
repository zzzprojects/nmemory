using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NMemory.DataStructures;
using NMemory.Tables;

namespace NMemory.Indexes
{
    public class RedBlackTreeIndexFactory : IndexFactoryBase
    {
        public RedBlackTreeIndexFactory(IKeyInfoFactory keyInfoFactory)
            : base(keyInfoFactory)
        {

        }

        public RedBlackTreeIndexFactory()
        {

        }

        public override IIndex<TEntity, TKey> CreateIndex<TEntity, TKey>(ITable<TEntity> table, IKeyInfo<TEntity, TKey> keyInfo)
        {
            RedBlackTree<TKey, TEntity> tree = new RedBlackTree<TKey, TEntity>(keyInfo.KeyComparer);
            Index<TEntity, TKey> index = new Index<TEntity, TKey>(table, keyInfo, tree);

            return index;
        }

        public override IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TEntity, TUniqueKey>(ITable<TEntity> table, IKeyInfo<TEntity, TUniqueKey> keyInfo)
        {
            UniqueRedBlackTree<TUniqueKey, TEntity> tree = new UniqueRedBlackTree<TUniqueKey, TEntity>(keyInfo.KeyComparer);
            UniqueIndex<TEntity, TUniqueKey> index = new UniqueIndex<TEntity, TUniqueKey>(table, keyInfo, tree);

            return index;
        }
    }
}
