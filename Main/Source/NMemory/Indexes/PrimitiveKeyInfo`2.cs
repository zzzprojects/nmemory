using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace NMemory.Indexes
{
    public class PrimitiveKeyInfo<TEntity, TKey> : KeyInfoBase<TEntity, TKey>, IKeyInfoExpressionBuilderProvider
        where TEntity : class
    {

        public PrimitiveKeyInfo(Expression<Func<TEntity, TKey>> keySelector, SortOrder sortOrder) : 
            base(
                GetEntityKeyMembers(keySelector),
                new SortOrder[1] { sortOrder },
                Comparer<TKey>.Default,
                keySelector.Compile(),
                PrimitiveKeyInfo<TEntity, TKey>.IsKeyEmpty)
        {

        }

        public PrimitiveKeyInfo(Expression<Func<TEntity, TKey>> keySelector) :
            this(keySelector, SortOrder.Ascending)
        {

        }

        private static MemberInfo[] GetEntityKeyMembers(Expression<Func<TEntity, TKey>> keySelector)
        {
            MemberExpression member = keySelector.Body as MemberExpression;

            if (member == null)
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            if (member.Expression.NodeType != ExpressionType.Parameter)
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            return new MemberInfo[] { member.Member };
        }

        private static bool IsKeyEmpty(TKey key)
        {
            return key == null;
        }

        IKeyInfoExpressionBuilder IKeyInfoExpressionBuilderProvider.KeyInfoExpressionBuilder
        {
            get 
            { 
                return new PrimitiveKeyInfoExpressionBuilder(typeof(TKey)); 
            }
        }
    }
}
