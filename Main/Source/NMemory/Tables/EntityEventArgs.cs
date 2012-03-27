using System;

namespace NMemory.Tables
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityEventArgs<TEntity> : EventArgs
    {
        public EntityEventArgs(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity
        {
            get;
            set;
        }
    }
}
