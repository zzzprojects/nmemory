// ----------------------------------------------------------------------------------
// <copyright file="Index.cs" company="NMemory Team">
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

namespace NMemory.Indexes
{
    using System.Collections.Generic;
    using NMemory.DataStructures;
    using NMemory.Tables;

    public class Index<TEntity, TKey> : IndexBase<TEntity, TKey>
        where TEntity : class
    {
        private IDataStructure<TKey, TEntity> dataStructure;

        #region Ctor

		public Index(
            ITable<TEntity> table, 
            IKeyInfo<TEntity, TKey> keyInfo, 
            IDataStructure<TKey, TEntity> dataStructure)
                
            : base(table, keyInfo)
        {
            this.dataStructure = dataStructure;

            this.Rebuild();
        }

        #endregion

        internal override IDataStructure<TKey, TEntity> DataStructure
        {
            get { return dataStructure; }
        }

        public override IEnumerable<TEntity> Select(TKey key)
        {
            return this.dataStructure.Select(key);
        }

        public override IEnumerable<TEntity> SelectAll()
        {
            return this.dataStructure.SelectAll();
        }

        public override void Insert(TEntity entity)
        {
            TKey key = Key(entity);

            dataStructure.Insert(key, entity);
        }

        public override void Delete(TEntity entity)
        {
            TKey i = Key(entity);

            this.dataStructure.Delete(i, entity);
        }

        #region Properties

        /// <summary>
        /// Gets a value that indicates whether the index structure supports interval search.
        /// </summary>
        public override bool SupportsIntervalSearch
        {
            get { return dataStructure.SupportsIntervalSearch; }
        }

        #endregion
    }
}    
