// ----------------------------------------------------------------------------------
// <copyright file="UniqueHashtable.cs" company="NMemory Team">
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
    using NMemory.Exceptions;

    public class UniqueHashtable<TKey, TEntity> : IUniqueDataStructure<TKey, TEntity>
        where TEntity : class
    {
        private Dictionary<TKey, TEntity> inner;

        public UniqueHashtable()
        {
            this.inner = new Dictionary<TKey, TEntity>();
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
            return new TEntity[] { this.inner[key] };
        }

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

        TEntity IUniqueDataStructure<TKey, TEntity>.Select(TKey key)
        {
            TEntity result = null;

            this.inner.TryGetValue(key, out result);

            return result;
        }

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
    }
}
