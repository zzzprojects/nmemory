// ----------------------------------------------------------------------------------
// <copyright file="UniqueRedBlackTree.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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
    using NMemory.Exceptions;

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

            this.Count = 0;
        }

        #region Properties

        public long Count 
        { 
            get; 
            private set; 
        }

        public bool SupportsIntervalSearch
        {
            get { return true; }
        }

        IEnumerable<TKey> IDataStructure<TKey, TEntity>.AllKeys
        {
            get
            {
                return this.inner.Keys;
            }
        }

        #endregion

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
