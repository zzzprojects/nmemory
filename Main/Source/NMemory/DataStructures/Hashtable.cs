// ----------------------------------------------------------------------------------
// <copyright file="Hashtable.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Hashtable<TKey, TEntity> : IDataStructure<TKey, TEntity>
    {
        private Dictionary<TKey, List<TEntity>> inner;

        public Hashtable()
        {
            this.inner = new Dictionary<TKey, List<TEntity>>();
            this.Count = 0;
        }

        public long Count { get; private set; }

        IEnumerable<TKey> IDataStructure<TKey, TEntity>.AllKeys
        {
            get { return this.inner.Keys; }
        }

        public bool SupportsIntervalSearch
        {
            get { return false; }
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
            foreach (List<TEntity> values in this.inner.Values)
            {
                foreach (TEntity entity in values)
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
