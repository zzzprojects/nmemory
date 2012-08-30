using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.DataStructures
{
	public class Hashtable<TKey, TEntity> : IDataStructure<TKey, TEntity>
	{
        private Dictionary<TKey, List<TEntity>> inner;

        public long Count { get; private set; }

		public Hashtable()
		{
            this.inner = new Dictionary<TKey, List<TEntity>>();
            Count = 0;
		}

		IEnumerable<TKey> IDataStructure<TKey, TEntity>.AllKeys
		{
			get
			{
				return this.inner.Keys;
			}
		}

		IEnumerable<TEntity> IDataStructure<TKey, TEntity>.Select(TKey key)
		{
            List<TEntity> result = null;

            if (this.inner.TryGetValue(key, out result))
            {
                return result;
            }
            else
            {
                return new TEntity[] { };
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

        public bool TryInsert(TKey key, TEntity entity)
        {
            this.Insert(key, entity);

            return true;
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
			get
			{
				return false;
			}
		}

		public IEnumerable<TEntity> Select(TKey from, TKey to, bool fromOpen, bool toOpen)
		{
			throw new NotSupportedException();
		}

		public IEnumerable<TEntity> SelectGreater(TKey from, bool open)
		{
            throw new NotSupportedException();
		}

		public IEnumerable<TEntity> SelectLess(TKey to, bool open)
		{
            throw new NotSupportedException();
		}

    }
}
