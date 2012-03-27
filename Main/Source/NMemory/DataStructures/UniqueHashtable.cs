using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Exceptions;

namespace NMemory.DataStructures
{
    public class UniqueHashtable<TKey, TEntity> : IUniqueDataStructure<TKey, TEntity>
        where TEntity : class
    {
        private Dictionary<TKey, TEntity> inner;

        public long Count { get; private set; }

        public UniqueHashtable()
        {
            this.inner = new Dictionary<TKey, TEntity>();
            Count = 0;
        }


        #region IDataStructure<TKey,TEntity> Members

        IEnumerable<TKey> IDataStructure<TKey, TEntity>.AllKeys
        {
            get
            {
                return this.inner.Keys;
            }
        }

        IEnumerable<TEntity> IDataStructure<TKey, TEntity>.Select(TKey key)
        {
            return new TEntity[] { this.inner[key] };
        }

        //IEnumerable<TEntity> IDataStructure<TKey, TEntity>.Select(Func<TKey, bool> whereKey)
        //{
        //    foreach(TKey key in this.Keys)
        //    {
        //        if(whereKey(key))
        //        {
        //            yield return this[key];
        //        }
        //    }
        //}

        public IEnumerable<TEntity> SelectAll()
        {
            return this.inner.Values;
        }

        public void Insert(TKey key, TEntity entity)
        {
            if (this.inner.ContainsKey(key))
            {
                throw new MultipleUniqueKeyFoundException();
            }
            else
            {
                this.inner.Add(key, entity);
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

        #endregion

        #region IUniqueDataStructure<TKey,TEntity> Members

        TEntity IUniqueDataStructure<TKey, TEntity>.Select(TKey key)
        {
            TEntity result = null;

            this.inner.TryGetValue(key, out result);

            return result;
        }

        #region Properties

        /// <summary>
        /// Megadja, hogy a adatmodfell támogadja-e az intervallumos keresést
        /// </summary>
        public bool SupportsIntervalSearch
        {
            get
            {
                return false;
            }
        }

        #endregion


        #endregion

        #region IIntervalSearchable<TKey,TEntity> Members

        public IEnumerable<TEntity> Select(TKey from, TKey to, bool fromOpen, bool toOpen)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> SelectGreater(TKey from, bool open)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> SelectLess(TKey to, bool open)
        {
            throw new NotImplementedException();
        }

        #endregion




    }
}
