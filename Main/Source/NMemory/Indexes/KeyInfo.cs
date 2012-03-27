using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NMemory.Indexes
{
    public class KeyInfo<TEntity, TKey> : IKeyInfo<TEntity, TKey>
    {
        private MemberInfo[] keyMembers;
        private Func<TEntity, TKey> keyAccess;

        public KeyInfo(Expression<Func<TEntity, TKey>> expression)
        {
            this.keyMembers = typeof(TKey).GetProperties().ToArray();
            this.keyAccess = expression.Compile();
        }

        public Type KeyType
        {
            get { return typeof(TKey); }
        }

        public MemberInfo[] KeyMembers
        {
            get { return keyMembers; }
        }

        public TKey GetKey(TEntity entity)
        {
            return this.keyAccess.Invoke(entity);
        }
    }
}
