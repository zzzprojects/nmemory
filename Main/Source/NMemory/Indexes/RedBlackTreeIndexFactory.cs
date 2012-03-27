using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NMemory.DataStructures;
using NMemory.Tables;

namespace NMemory.Indexes
{
    public class RedBlackTreeIndexFactory<TEntity> : IIndexFactory<TEntity>
        where TEntity : class
    {
        public IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, Expression<Func<TEntity, TKey>> key)
        {
            RedBlackTree<TKey, TEntity> tree = new RedBlackTree<TKey, TEntity>();
            Index<TEntity, TKey> index = new Index<TEntity, TKey>(table, key, tree);

            return index;
        }

        public IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, Expression<Func<TEntity, TKey>> key, IComparer<TKey> comparer)
        {
            RedBlackTree<TKey, TEntity> tree = new RedBlackTree<TKey, TEntity>(comparer);
            Index<TEntity, TKey> index = new Index<TEntity, TKey>(table, key, tree);

            return index;
        }

        public UniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, Expression<Func<TEntity, TUniqueKey>> key)
        {
            UniqueRedBlackTree<TUniqueKey, TEntity> tree = new UniqueRedBlackTree<TUniqueKey, TEntity>();
            UniqueIndex<TEntity, TUniqueKey> index = new UniqueIndex<TEntity, TUniqueKey>(table, key, tree);

            return index;
        }

        public UniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, Expression<Func<TEntity, TUniqueKey>> key, IComparer<TUniqueKey> comparer)
        {
            UniqueRedBlackTree<TUniqueKey, TEntity> tree = new UniqueRedBlackTree<TUniqueKey, TEntity>(comparer);
            UniqueIndex<TEntity, TUniqueKey> index = new UniqueIndex<TEntity, TUniqueKey>(table, key, tree);

            return index;
        }
    }
}
