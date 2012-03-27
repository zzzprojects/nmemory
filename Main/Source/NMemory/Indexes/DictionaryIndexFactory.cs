using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NMemory.DataStructures;
using NMemory.Tables;

namespace NMemory.Indexes
{
    public class DictionaryIndexFactory<TEntity> : IIndexFactory<TEntity>
        where TEntity : class
    {
        public IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, Expression<Func<TEntity, TKey>> key)
        {
            Hashtable<TKey, TEntity> hashTable = new Hashtable<TKey, TEntity>();
            Index<TEntity, TKey> index = new Index<TEntity, TKey>(table, key, hashTable);

            return index;
        }

        public IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, Expression<Func<TEntity, TKey>> key, IComparer<TKey> keyComparer)
        {
            Hashtable<TKey, TEntity> hashTable = new Hashtable<TKey, TEntity>(); // keycomparer?
            Index<TEntity, TKey> index = new Index<TEntity, TKey>(table, key, hashTable);

            return index;
        }

        public UniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, Expression<Func<TEntity, TUniqueKey>> key)
        {
            UniqueHashtable<TUniqueKey, TEntity> hashTable = new UniqueHashtable<TUniqueKey, TEntity>();
            UniqueIndex<TEntity, TUniqueKey> index = new UniqueIndex<TEntity, TUniqueKey>(table, key, hashTable);

            return index;
        }

        public UniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, Expression<Func<TEntity, TUniqueKey>> key, IComparer<TUniqueKey> keyComparer)
        {
            UniqueHashtable<TUniqueKey, TEntity> hashTable = new UniqueHashtable<TUniqueKey, TEntity>(); // keycomparer?
            UniqueIndex<TEntity, TUniqueKey> index = new UniqueIndex<TEntity, TUniqueKey>(table, key, hashTable);

            return index;
        }

    }
}
