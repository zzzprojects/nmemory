using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;

namespace NMemory.Transactions.Logs
{
    internal class IndexDeleteTransactionLogItem<TEntity> : IndexTransactionLogItemBase<TEntity>
        where TEntity : class
    {
        public IndexDeleteTransactionLogItem(IIndex<TEntity> index, TEntity entity) 
            : base(index, entity)
        {
        }

        public override void Undo()
        {
            this.Index.Insert(this.Entity);
        }
    }
}
