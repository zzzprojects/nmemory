using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;

namespace NMemory.Indexes
{
    public interface IIndex<TEntity> : IIndex
        where TEntity : class
    {
        IEnumerable<TEntity> SelectAll();

        new ITable<TEntity> Table { get; }

        void Insert(TEntity item);

        void Delete(TEntity item);
    }
}
