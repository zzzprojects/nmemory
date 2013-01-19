// ----------------------------------------------------------------------------------
// <copyright file="IndexBase.cs" company="NMemory Team">
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
    using System.Linq;
    using System.Reflection;
    using NMemory.DataStructures;
    using NMemory.Linq;
    using NMemory.Tables;

    public abstract class IndexBase<TEntity, TKey> : IIndex<TEntity, TKey>
        where TEntity : class
    {
        #region Fields

        private ITable<TEntity> table;
        private IKeyInfo<TEntity, TKey> keyInfo;

        #endregion

        #region Ctor

        internal IndexBase(ITable<TEntity> table, IKeyInfo<TEntity, TKey> keyInfo) 
        {
            this.table = table;
            this.keyInfo = keyInfo;
        }

        #endregion

        #region IIndex<TEntity,TPrimaryKey,TKey,TDefaultTrafo> Members

        public TKey Key(TEntity entity)
        {
            return this.keyInfo.SelectKey(entity);
        }

        public abstract IEnumerable<TEntity> Select(TKey value);

        public IEnumerable<TEntity> Select(TKey from, TKey to, bool fromOpen, bool toOpen)
        {
            return this.DataStructure.Select(from, to, fromOpen, toOpen);
        }

        public IEnumerable<TEntity> SelectGreater(TKey from, bool open)
        {
            return this.DataStructure.SelectGreater(from, open);
        }

        public IEnumerable<TEntity> SelectLess(TKey to, bool open)
        {
            return this.DataStructure.SelectLess(to, open);
        }

        public abstract IEnumerable<TEntity> SelectAll();

        public abstract void Insert(TEntity item);

        public abstract void Delete(TEntity item);

        #endregion

        #region Query expression

        public IQueryable<TEntity> SelectExpr(TKey epxr)
        {
            TKey key = (TKey)epxr;

            return Select(key).AsIndexedQueryable(this);
        }

        public IQueryable<TEntity> SelectExpr(TKey from, TKey to, bool fromOpen, bool toOpen)
        {
            TKey fromKey = (TKey)from;
            TKey toKey = (TKey)to;

            return this.Select(fromKey, toKey, fromOpen, toOpen).AsIndexedQueryable(this);
        }

        public IQueryable<TEntity> SelectGreaterExpr(TKey from, bool open)
        {
            TKey key = (TKey)from;

            return this.SelectGreater(key, open).AsIndexedQueryable(this);
        }

        public IQueryable<TEntity> SelectLessExpr(TKey from, bool open)
        {
            TKey key = (TKey)from;

            return SelectLess(key, open).AsIndexedQueryable(this);
        }

        #endregion

        #region IIndex Members

        public IKeyInfo<TEntity, TKey> KeyInfo
        {
            get { return this.keyInfo; }
        }

        IKeyInfo IIndex.KeyInfo
        {
            get { return this.keyInfo; }
        }

        public ITable<TEntity> Table
        {
            get { return this.table; }
        }

        ITable IIndex.Table
        {
            get { return this.table; }
        }

        public abstract bool SupportsIntervalSearch
        {
            get;
        }

        public long Count
        {
            get { return this.DataStructure.Count; }
        }

        #endregion

        internal abstract IDataStructure<TKey, TEntity> DataStructure
        {
            get;
        }

        public override string ToString()
        {
            return string.Format("{0} | {1} {2} | TABLE: {3}", 
                this.GetType().Name, 
                (this.KeyInfo.EntityKeyMembers.Length > 1) ? "COLUMNS" : "COLUMN",
                string.Join(", ", this.KeyInfo.EntityKeyMembers.Select(mi => 
                    string.Format("{0} ({1})",
                        mi.Name, 
                        (mi.MemberType == MemberTypes.Property) ? 
                            ((PropertyInfo)mi).PropertyType.Name : 
                            mi.MemberType.ToString()))
                    .ToArray()), table);
        }

        public bool Contains(TKey key)
        {
            return this.DataStructure.ContainsKey(key);
        }

        public void Rebuild()
        {
            if (this.Table.PrimaryKeyIndex == null)
            {
                // Primary index has not been initialized yet
                // Probably we are in the constructor of the Table class now
                return;
            }

            IEnumerable<TEntity> tableData = this.Table.PrimaryKeyIndex.SelectAll();

            if (this.Table.PrimaryKeyIndex == this)
            {
                // Store the current data, if the primary index is being rebuilt
                tableData = tableData.ToList();
            }

            this.DataStructure.Clear();

            foreach (TEntity entity in tableData)
            {
                this.DataStructure.Insert(this.KeyInfo.SelectKey(entity), entity);
            }
        }
    }
}
