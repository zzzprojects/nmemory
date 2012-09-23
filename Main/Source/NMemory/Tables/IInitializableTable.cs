using System.Collections.Generic;

namespace NMemory.Tables
{
    public interface IInitializableTable<TEntity>
        where TEntity : class
    {
        void Initialize(IEnumerable<TEntity> initialData);
    }
}
