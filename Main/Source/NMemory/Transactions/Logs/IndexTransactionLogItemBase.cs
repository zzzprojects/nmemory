using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;

namespace NMemory.Transactions.Logs
{
    internal abstract class IndexTransactionLogItemBase<TEntity> : ITransactionLogItem
    {
        private IIndex<TEntity> index;
        private TEntity entity;

        public IndexTransactionLogItemBase(IIndex<TEntity> index, TEntity entity)
        {
            this.index = index;
            this.entity = entity;
        }

        protected IIndex<TEntity> Index
        {
            get { return this.index; }
        }

        protected TEntity Entity
        {
            get { return this.entity; }
        }

        public abstract void Undo();
    }
}
