using System;

namespace NMemory.Tables
{
    public class EntityUpdateEventArgs<TEntity> : EventArgs
    {
        public EntityUpdateEventArgs(TEntity oldEntity, TEntity newEntity)
        {
            this.OldEntity = oldEntity;
            this.NewEntity = newEntity;
        }
        
        public TEntity OldEntity
        {
            get;
            private set;
        }

        public TEntity NewEntity
        {
            get;
            private set;
        }
    }
}
