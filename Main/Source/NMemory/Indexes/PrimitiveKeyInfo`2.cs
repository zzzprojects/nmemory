using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace NMemory.Indexes
{
    public class PrimitiveKeyInfo<TEntity, TKey> : KeyInfoBase<TEntity, TKey>
        where TEntity : class
    {

        public PrimitiveKeyInfo(Expression<Func<TEntity, TKey>> keySelector) : 
            base(
                GetEntityKeyMembers(keySelector),
                Comparer<TKey>.Default,
                keySelector.Compile(),
                PrimitiveKeyInfo<TEntity, TKey>.IsKeyEmpty)
        {

        }

        private static MemberInfo[] GetEntityKeyMembers(Expression<Func<TEntity, TKey>> keySelector)
        {
            MemberExpression memberSelection = keySelector.Body as MemberExpression;

            if (memberSelection == null)
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            return new MemberInfo[] { memberSelection.Member };
        }

        private static bool IsKeyEmpty(TKey key)
        {
            return key == null;
        }


    }
}
