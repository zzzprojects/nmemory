using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Exceptions;
using NMemory.Transactions;
using NMemory.DataStructures;
using System.Linq.Expressions;
using System.Diagnostics;
using NMemory.Tables;

namespace NMemory.Indexes
{
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
