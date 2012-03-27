using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Exceptions;

namespace NMemory.DataStructures
{
    public class UniqueRedBlackTree<TKey, TEntity> : IUniqueDataStructure<TKey, TEntity>
         where TEntity : class
    {
        private Internal.Trees.RedBlackTree<TKey, TEntity> inner;

        public UniqueRedBlackTree() : this(Comparer<TKey>.Default)
        {

        }

        public UniqueRedBlackTree(IComparer<TKey> comparer)
        {
            this.inner = new Internal.Trees.RedBlackTree<TKey, TEntity>(comparer);

            Count = 0;
        }

        public long Count 
        { 
            get; 
            private set; 
        }


        IEnumerable<TKey> IDataStructure<TKey, TEntity>.AllKeys
        {
            get
            {
                return this.inner.Keys;
            }
        }

        TEntity IUniqueDataStructure<TKey, TEntity>.Select(TKey key)
        {
            TEntity result = null;

            if (this.inner.TryGetValue(key, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        IEnumerable<TEntity> IDataStructure<TKey, TEntity>.Select(TKey key)
        {
            TEntity result = null;

            if (this.inner.TryGetValue(key, out result))
            {
                return new TEntity[] { result };
            }
            else
            {
                return new TEntity[0];
            }
        }

        public IEnumerable<TEntity> SelectAll()
        {
            foreach (TEntity entity in this.inner.Values)
            {
                yield return entity;
            }
        }

        public void Insert(TKey key, TEntity entity)
        {
            // TODO: TryAdd
            if (!this.inner.TryAdd(key, entity))
            {
                throw new MultipleUniqueKeyFoundException();
            }

            this.Count++;
        }

        public void Delete(TKey key, TEntity item)
        {
            if (this.inner.Remove(key))
            {
                this.Count--;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.inner.ContainsKey(key);
        }

        public void Clear()
        {
            this.inner.Clear();
            this.Count = 0;
        }


        public bool SupportsIntervalSearch
        {
            get { return true; }
        }

        public IEnumerable<TEntity> Select(TKey from, TKey to, bool fromOpen, bool toOpen)
        {
            return this.inner.IntervalSearch(from, to, fromOpen, toOpen);
        }

        public IEnumerable<TEntity> SelectGreater(TKey from, bool open)
        {
            return this.inner.SearchGreater(from, open);
        }

        public IEnumerable<TEntity> SelectLess(TKey to, bool open)
        {
            return this.inner.SearchLess(to, open);
        }
    }
}
