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
        public TupleKeyInfo(Expression<Func<TEntity, TKey>> keyExpression, SortOrder[] sortOrders) : 
            base(
                GetEntityKeyMembers(keyExpression),
                sortOrders,
                Comparer<TKey>.Default,
                keyExpression.Compile(),
                TupleKeyInfo<TEntity, TKey>.CreateKeyEmptinessDetector())
        {

        }

        public TupleKeyInfo(Expression<Func<TEntity, TKey>> keyExpression) :
            base(
                GetEntityKeyMembers(keyExpression),
                GetDefaultSortOrders(keyExpression),
                Comparer<TKey>.Default,
                keyExpression.Compile(),
                TupleKeyInfo<TEntity, TKey>.CreateKeyEmptinessDetector())
        {

        }

        private static SortOrder[] GetDefaultSortOrders(Expression<Func<TEntity, TKey>> keySelector)
        {
            int fieldCount = GetEntityKeyMembers(keySelector).Length;

            return Enumerable.Repeat(SortOrder.Ascending, fieldCount).ToArray();
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
