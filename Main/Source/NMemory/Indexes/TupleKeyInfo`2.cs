using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace NMemory.Indexes
{
    public class TupleKeyInfo<TEntity, TKey> : KeyInfoBase<TEntity, TKey>
        where TEntity : class
    {
        public TupleKeyInfo(Expression<Func<TEntity, TKey>> keyExpression) : 
            base(
                GetEntityKeyMembers(keyExpression),
                Comparer<TKey>.Default,
                keyExpression.Compile(),
                TupleKeyInfo<TEntity, TKey>.CreateKeyEmptinessDetector())
        {

        }

        private static MemberInfo[] GetKeyMembers(Expression<Func<TEntity, TKey>> keyExpression)
        {
            throw new NotImplementedException();
        }

        private static MemberInfo[] GetEntityKeyMembers(Expression<Func<TEntity, TKey>> keyExpression)
        {
            throw new NotImplementedException();
        }

        private static Func<TKey, bool> CreateKeyEmptinessDetector()
        {
            throw new NotImplementedException();
        }


    }
}
