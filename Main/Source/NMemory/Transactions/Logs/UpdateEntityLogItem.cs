using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;

namespace NMemory.Transactions.Logs
{
    internal class UpdateEntityLogItem<TEntity> : ITransactionLogItem
    {
        private TEntity storedEntity;
        private TEntity oldEntity;
        private EntityPropertyCloner<TEntity> propertyCloner;

        public UpdateEntityLogItem(EntityPropertyCloner<TEntity> propertyCloner, TEntity storedEntity, TEntity oldEntity)
        {
            this.storedEntity = storedEntity;
            this.oldEntity = oldEntity;
            this.propertyCloner = propertyCloner;
        }

        public void Undo()
        {
            this.propertyCloner.Clone(oldEntity, storedEntity);
        }
    }
}
