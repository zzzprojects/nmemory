using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Tables
{
    public interface IInitializableTable<TEntity>
        where TEntity : class
    {
        void Initialize(IEnumerable<TEntity> initialData);
    }
}
