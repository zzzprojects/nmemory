using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using System.Linq.Expressions;
using NMemory.DataStructures;
using NMemory.Common;

namespace NMemory.Indexes
{
    public abstract class IndexFactoryBase<TEntity> : IIndexFactory<TEntity> where TEntity : class
    {
        public IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, Expression<Func<TEntity, TKey>> key)
        {
            // Check if a custom comparer is needed
            IComparer<TKey> comparer;

            if (!CreateComparerIfNeeded<TKey>(out comparer))
            {
                comparer = Comparer<TKey>.Default;
            }

            return CreateIndex(table, key, comparer); 
        }

        public IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, Expression<Func<TEntity, TUniqueKey>> key)
        {
            // Check if a custom comparer is needed
            IComparer<TUniqueKey> comparer;

            if (!CreateComparerIfNeeded<TUniqueKey>(out comparer))
            {
                comparer = Comparer<TUniqueKey>.Default;
            }

            return CreateUniqueIndex(table, key, comparer);
        }

        public abstract IIndex<TEntity, TKey> CreateIndex<TKey>(ITable<TEntity> table, Expression<Func<TEntity, TKey>> key, IComparer<TKey> keyComparer);

        public abstract IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(ITable<TEntity> table, Expression<Func<TEntity, TUniqueKey>> key, IComparer<TUniqueKey> keyComparer);

        private bool CreateComparerIfNeeded<TKey>(out IComparer<TKey> comparer)
        {
            comparer = null;

            if (ReflectionHelper.IsAnonymousType(typeof(TKey)))
            {
                comparer = new AnonymousTypeComparer<TKey>();
                return true;
            }

            return false;
        }
    }
}
