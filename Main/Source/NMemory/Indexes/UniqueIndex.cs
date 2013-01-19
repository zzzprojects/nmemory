// ----------------------------------------------------------------------------------
// <copyright file="UniqueIndex.cs" company="NMemory Team">
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

namespace NMemory.Indexes
{
    using System.Collections.Generic;
    using NMemory.DataStructures;
    using NMemory.Tables;

    public class UniqueIndex<TEntity, TUniqueKey> : IndexBase<TEntity, TUniqueKey>, IUniqueIndex<TEntity, TUniqueKey>
        where TEntity : class
    {
        private IUniqueDataStructure<TUniqueKey, TEntity> uniqueDataStructure;

        #region Ctor

        internal UniqueIndex(
            ITable<TEntity> table, 
            IKeyInfo<TEntity, TUniqueKey> keyInfo, 
            IUniqueDataStructure<TUniqueKey, TEntity> dataStructure)
            : base(table, keyInfo)
        {
            this.uniqueDataStructure = dataStructure;

            this.Rebuild();
        }

        #endregion

        #region Methods

        public TEntity GetByUniqueIndex(TUniqueKey indexKeys)
        {
            return this.uniqueDataStructure.Select(indexKeys);
        }

        public override void Insert(TEntity item)
        {
            TUniqueKey key = Key(item);

            this.uniqueDataStructure.Insert(key, item);
        }

        public override void Delete(TEntity item)
        {
            TUniqueKey key = Key(item);
            
            this.uniqueDataStructure.Delete(key, item);
        }

        internal override IDataStructure<TUniqueKey, TEntity> DataStructure
        {
            get { return uniqueDataStructure; }
        }

        #endregion

        #region IIndex<TEntity,TUniqueKey> Members

        public override IEnumerable<TEntity> Select(TUniqueKey value)
        {
            TEntity selected = this.uniqueDataStructure.Select(value);

            if (selected != null)
            {
                yield return selected;
            }
        }

        public override IEnumerable<TEntity> SelectAll()
        {
            return uniqueDataStructure.SelectAll();
        }

        #endregion

        #region Properties

        public override bool SupportsIntervalSearch
        {
            get { return DataStructure.SupportsIntervalSearch; }
        }

        #endregion
    }
}
