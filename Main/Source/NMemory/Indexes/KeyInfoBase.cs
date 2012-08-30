using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NMemory.Indexes
{
    public abstract class KeyInfoBase<TEntity, TKey> : IKeyInfo<TEntity, TKey>
        where TEntity : class
    {
        private MemberInfo[] entityKeyMembers;
        private SortOrder[] sortOrders;

        private IComparer<TKey> keyComparer;
        private Func<TEntity, TKey> keySelector;
        private Func<TKey, bool> keyEmptinessDetector;

        public KeyInfoBase(
            MemberInfo[] entityKeyMembers,
            SortOrder[] sortOrders,

            IComparer<TKey> keyComparer,
            Func<TEntity, TKey> keySelector,
            Func<TKey, bool> keyEmptinessDetector)
        {
            this.entityKeyMembers = entityKeyMembers;
            this.sortOrders = sortOrders;

            this.keyComparer = keyComparer;
            this.keySelector = keySelector;
            this.keyEmptinessDetector = keyEmptinessDetector;
        }

        public TKey SelectKey(TEntity entity)
        {
            return this.keySelector.Invoke(entity);
        }

        public bool IsEmptyKey(TKey key)
        {
            return this.keyEmptinessDetector.Invoke(key);
        }

        public IComparer<TKey> KeyComparer
        {
            get { return this.keyComparer; }
        }

        public MemberInfo[] EntityKeyMembers
        {
            get { return this.entityKeyMembers.ToArray(); }
        }

        public SortOrder[] SortOrders
        {
            get { return this.sortOrders.ToArray(); }
        }

        public Type KeyType
        {
            get { return typeof(TKey); }
        }
    }
}
