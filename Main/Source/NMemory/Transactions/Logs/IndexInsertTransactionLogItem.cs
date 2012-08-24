using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;

namespace NMemory.Transactions.Logs
{
    internal class IndexInsertTransactionLogItem<TEntity> : IndexTransactionLogItemBase<TEntity>
        where TEntity : class
    {
        public IndexInsertTransactionLogItem(IIndex<TEntity> index, TEntity entity) 
            : base(index, entity)
        {
        }

        public override void Undo()
        {
            this.Index.Delete(this.Entity);
        }
    }
}
