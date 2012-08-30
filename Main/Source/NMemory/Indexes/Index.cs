using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Transactions;
using NMemory.DataStructures;
using System.Linq.Expressions;
using NMemory.Tables;
using NMemory.Exceptions;

namespace NMemory.Indexes
{
	public class Index<TEntity, TKey> : IndexBase<TEntity, TKey>
		where TEntity : class
	{
		private IDataStructure<TKey, TEntity> dataStructure;

		#region Ctor

		internal Index(
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
