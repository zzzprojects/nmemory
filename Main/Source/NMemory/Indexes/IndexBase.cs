using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.DataStructures;
using NMemory.Linq;
using NMemory.Tables;

namespace NMemory.Indexes
{

	public abstract class IndexBase<TEntity, TKey> : IIndex<TEntity, TKey>, IIndex
		where TEntity : class
	{
		#region Fields

		private ITable<TEntity> table;
		private IKeyInfo<TEntity, TKey> keyInfo;

		#endregion

		#region Ctor

		internal IndexBase(ITable<TEntity> table, Expression<Func<TEntity, TKey>> key)
		{
			if (typeof(TKey).IsValueType || (typeof(TKey) == typeof(string)))
			{
				this.keyInfo = new SimpleKeyInfo<TEntity, TKey>(key);
			}
			else
			{
				this.keyInfo = new KeyInfo<TEntity, TKey>(key);
			}

			this.table = table;
		}


		#endregion

		#region IIndex<TEntity,TPrimaryKey,TKey,TDefaultTrafo> Members

		public TKey Key(TEntity entity)
		{
			return this.keyInfo.GetKey(entity);
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
                (this.KeyInfo.KeyMembers.Length > 1) ? "COLUMNS" : "COLUMN",
                string.Join(", ", this.KeyInfo.KeyMembers.Select(mi => 
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
                tableData = tableData.ToList();
            }

            this.DataStructure.Clear();

            foreach (TEntity entity in tableData)
            {
                this.DataStructure.Insert(this.KeyInfo.GetKey(entity), entity);
            }
        }
    }
}
