using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;
using System.Threading;
using NMemory.Tables;

namespace NMemory.Transactions.Logs
{
    internal class TransactionLog
    {
        private List<ITransactionLogItem> logItems;

        public TransactionLog()
        {
            this.logItems = new List<ITransactionLogItem>();
        }

        public int CurrentPosition
        {
            get { return this.logItems.Count; }
        }

        public void Rollback()
        {
            this.RollbackTo(0);
        }

        public void RollbackTo(int position)
        {
            for (int i = this.logItems.Count - 1; i >= position; i--)
            {
                this.logItems[i].Undo();
                this.logItems.RemoveAt(i);
            }
        }

        public void Release()
        {
            this.logItems.Clear();
        }

        public void WriteIndexInsert<TEntity>(IIndex<TEntity> index, TEntity entity)
        {
            this.logItems.Add(new IndexInsertTransactionLogItem<TEntity>(index, entity));
        }

        public void WriteIndexDelete<TEntity>(IIndex<TEntity> index, TEntity entity)
        {
            this.logItems.Add(new IndexDeleteTransactionLogItem<TEntity>(index, entity));
        }

        public void WriteEntityUpdate<TEntity>(EntityPropertyCloner<TEntity> propertyCloner, TEntity storedEntity, TEntity oldEntity)
        {
            this.logItems.Add(new UpdateEntityLogItem<TEntity>(propertyCloner, storedEntity, oldEntity));
        }
    }
}
