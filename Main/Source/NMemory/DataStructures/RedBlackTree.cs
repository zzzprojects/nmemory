using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.DataStructures
{
	public class RedBlackTree<TKey, TEntity> : IDataStructure<TKey, TEntity>
	{
        private Internal.Trees.RedBlackTree<TKey, List<TEntity>> inner;

        public RedBlackTree() 
            : this(Comparer<TKey>.Default)
        {

        }

        public RedBlackTree(IComparer<TKey> comparer)
        {
            this.inner = new Internal.Trees.RedBlackTree<TKey, List<TEntity>>(comparer);
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

        public IEnumerable<TEntity> Select(TKey key)
		{
            List<TEntity> result = null;

            if (this.inner.TryGetValue(key, out result))
            {
                return result;
            }
            else
            {
                return new TEntity[0];
            }
		}

		public IEnumerable<TEntity> SelectAll()
		{
			foreach(List<TEntity> values in this.inner.Values)
			{
				foreach(TEntity entity in values)
				{
					yield return entity;
				}
			}
		}

		public void Insert(TKey key, TEntity entity)
		{
            List<TEntity> bucket = null;

            if (this.inner.TryGetValue(key, out bucket))
            {
                bucket.Add(entity);
            }
            else
            {
                this.inner.Add(key, new List<TEntity>() { entity });
            }

            this.Count++;
		}

		public void Delete(TKey key, TEntity item)
		{
            List<TEntity> bucket = null;

            if (this.inner.TryGetValue(key, out bucket))
            {
                if (bucket.Remove(item))
                {
                    if (bucket.Count == 0)
                    {
                        this.inner.Remove(key);
                    }

                    this.Count--;
                }
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
			foreach (List<TEntity> entities in this.inner.IntervalSearch(from, to, fromOpen, toOpen))
			{
                foreach (TEntity entity in entities)
                {
                    yield return entity;
                }
			}
		}

		public IEnumerable<TEntity> SelectGreater(TKey from, bool open)
		{
            foreach (List<TEntity> entities in this.inner.SearchGreater(from, open))
			{
                foreach (TEntity entity in entities)
                {
                    yield return entity;
                }
			}
		}

		public IEnumerable<TEntity> SelectLess(TKey to, bool open)
		{
            foreach (List<TEntity> entities in this.inner.SearchLess(to, open))
			{
                foreach (TEntity entity in entities)
                {
                    yield return entity;
                }
			}
		}

    }
}
