// ----------------------------------------------------------------------------------
// <copyright file="RedBlackTree.cs" company="NMemory Team">
//     Copyright (C) 2012 NMemory Team
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
            this.Count = 0;
        }

        #region Properties

        IEnumerable<TKey> IDataStructure<TKey, TEntity>.AllKeys
        {
            get
            {
                return this.inner.Keys;
            }
        }

        public long Count
        {
            get;
            private set;
        }

        public bool SupportsIntervalSearch
        {
            get { return true; }
        }

        #endregion

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
